#include <stdio.h>
#ifndef DISPATCHER_H
#include "dispatcher.h"
#endif

int main() {
    socket_t server_socket, client_socket;
    struct sockaddr_in server_addr, client_addr;
    socklen_t client_addr_len = sizeof(client_addr);

#ifdef _WIN32
    WSADATA wsa_data;
    if (WSAStartup(MAKEWORD(2, 2), &wsa_data) != 0) {
        fprintf(stderr, "Error during Winsock initialisation.\n");
        return EXIT_FAILURE;
    }
#endif

    // Créer un socket serveur
    if ((server_socket = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) {
        perror("Error while creating socket");
        return EXIT_FAILURE;
    }

    // Configurer l'adresse du serveur
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(PORT);

    // Attacher le socket à l'adresse et au port
    if (bind(server_socket, (struct sockaddr *)&server_addr, sizeof(server_addr)) == SOCKET_ERROR) {
        perror("Error while binding socket");
        CLOSE_SOCKET(server_socket);
        return EXIT_FAILURE;
    }

    // Écouter les connexions entrantes
    if (listen(server_socket, 5) == SOCKET_ERROR) {
        perror("Refused to listen on the socket");
        CLOSE_SOCKET(server_socket);
        return EXIT_FAILURE;
    }

    printf("Server now listening on port  %d...\n", PORT);

    // Boucle principale pour accepter et gérer les connexions clients
    while (1) {
        client_socket = accept(server_socket, (struct sockaddr *)&client_addr, &client_addr_len);
        if (client_socket == INVALID_SOCKET) {
            perror("Error while accepting connection");
            continue;
        }

        printf("New connection accepted\n");
        handle_client(client_socket);
    }

    CLOSE_SOCKET(server_socket);
#ifdef _WIN32
    WSACleanup();
#endif
    return 0;
}