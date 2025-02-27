//
// Created by maxim on 23/01/2025.
//
#ifndef DISPATCHER_H
#include "dispatcher.h"
#endif


void DISPATCHER(const char *program_name, const char *method_name, int *arg_count, const char **cobol_args, int *status) {
    char command[256]; // Buffer pour stocker la commande
    char args_str[256] = ""; // Buffer pour stocker les arguments concaténés

    // Concaténation des arguments
    for (int i = 0; i < *arg_count; i++) {
        strcat(args_str, cobol_args[i]);
        strcat(args_str, " ");
    }

    // Construction de la commande en fonction de la plateforme
#ifdef _WIN32
    snprintf(command, sizeof(command), "\"%s\" \"%s\" %d %s", program_name, method_name, *arg_count, args_str);
#else
    snprintf(command, sizeof(command), "%s %s %d %s", program_name, method_name, *arg_count, args_str);
#endif

    // Exécution de la commande
    *status = system(command);
}

THREAD_FUNC_RETURN dynamic_call_cobol(void *data) {
    DynamicCall *call = (DynamicCall *)data;
    int status = 0;

    // Préparer les arguments
    const char *cobol_args[MAX_ARGS];
    for (int i = 0; i < call->arg_count; i++) {
        cobol_args[i] = call->args[i];
    }

    // Appeler le programme COBOL via le dispatcher
    DISPATCHER(call->program_name, call->method_name, &call->arg_count, cobol_args, &status);

    if (status != 0) {
        fprintf(stderr, "Call error at %s -> %s, status %d\n",
                call->program_name, call->method_name, status);
    } else {
        printf("Call succeed : %s -> %s\n", call->program_name, call->method_name);
    }

    free(call);

#ifdef _WIN32
    return 0;
#else
    return NULL;
#endif
}

void handle_client(socket_t client_socket) {
    char buffer[BUFFER_SIZE];
    ssize_t received;

    // Lire les données envoyées par le client
#ifdef _WIN32
    received = recv(client_socket, buffer, BUFFER_SIZE - 1, 0);
#else
    received = read(client_socket, buffer, BUFFER_SIZE - 1);
#endif
    if (received <= 0) {
        perror("Error while receiving data from client");
        CLOSE_SOCKET(client_socket);
        return;
    }

    // Terminer la chaîne de caractères
    buffer[received] = '\0';
    printf("Send request : %s\n", buffer);

    // Parser les arguments
    char *token = strtok(buffer, " ");
    if (!token) {
        fprintf(stderr, "Error : unspecified COBOL program\n");
        CLOSE_SOCKET(client_socket);
        return;
    }

    // Allouer un DynamicCall
    DynamicCall *call = NULL;
    if ((call = (DynamicCall *)malloc(sizeof(DynamicCall))) == NULL) {
        perror("Memory allocation error");
        CLOSE_SOCKET(client_socket);
        return;
    }

    // Extraire le programme et la méthode
    strncpy(call->program_name, token, MAX_ARG_LENGTH);
    token = strtok(NULL, " ");
    if (!token) {
        fprintf(stderr, "Error : unspecified COBOL method\n");
        free(call);
        CLOSE_SOCKET(client_socket);
        return;
    }
    strncpy(call->method_name, token, MAX_ARG_LENGTH);

    // Extraire les arguments
    call->arg_count = 0;
    while ((token = strtok(NULL, " ")) != NULL && call->arg_count < MAX_ARGS) {
        strncpy(call->args[call->arg_count], token, MAX_ARG_LENGTH);
        call->arg_count++;
    }

    // Lancer un thread pour exécuter le COBOL
    pthread_t thread;
#ifdef _WIN32
    HANDLE thread_handle = CreateThread(NULL, 0, dynamic_call_cobol, call, 0, NULL);
    if (thread_handle == NULL) {
        fprintf(stderr, "Erreur lors de la création du thread.\n");
        free(call);
        CLOSE_SOCKET(client_socket);
        return;
    }
    CloseHandle(thread_handle);
#else
    if (pthread_create(&thread, NULL, dynamic_call_cobol, call) != 0) {
        perror("Erreur lors de la création du thread");
        free(call);
        CLOSE_SOCKET(client_socket);
        return;
    }
    pthread_detach(thread);
#endif

    // Répondre au client
    const char *response = "Requête reçue et en cours de traitement.\n";
#ifdef _WIN32
    send(client_socket, response, strlen(response), 0);
#else
    write(client_socket, response, strlen(response));
#endif

    CLOSE_SOCKET(client_socket);
}