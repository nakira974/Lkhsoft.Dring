//
// Created by maxim on 23/01/2025.
//

#ifndef DISPATCHER_H
#define DISPATCHER_H

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <pthread.h>

// Détection du système d'exploitation
#ifdef _WIN32
    #include <winsock2.h>
    #include <ws2tcpip.h>
    #pragma comment(lib, "ws2_32.lib")

    typedef SOCKET socket_t;
#define CLOSE_SOCKET closesocket
#define THREAD_FUNC_RETURN DWORD WINAPI
#else
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <errno.h>

typedef int socket_t;
#define INVALID_SOCKET (-1)
#define SOCKET_ERROR (-1)
#define CLOSE_SOCKET close
#define THREAD_FUNC_RETURN void *
#endif

/* Maximal number of arguments */
#define MAX_ARGS 10
/* Maximal length of an argument */
#define MAX_ARG_LENGTH 100
/* Server port */
#define PORT 9333
/* Buffer size */
#define BUFFER_SIZE 1024

/* Structure that encapsulates target program info */
typedef struct {
    char program_name[MAX_ARG_LENGTH];
    char method_name[MAX_ARG_LENGTH];
    int arg_count;
    char args[MAX_ARGS][MAX_ARG_LENGTH];
} DynamicCall;

/* COBOL dispatcher */
extern void DISPATCHER(const char *program_name, const char *method_name,
                       int *arg_count, char *args[], int *status);

/* Client handle for the server */
void handle_client(socket_t client_socket);

/* Dynamic call dispatcher */
THREAD_FUNC_RETURN dynamic_call_cobol(void *data);
#endif //DISPATCHER_H
