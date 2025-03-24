//
// Created by maxim on 16/03/2025.
//

#ifndef CIRCULAR_BUFFER_H
#define CIRCULAR_BUFFER_H
#include <stdbool.h>

#ifdef __cplusplus

extern "C" {
#endif
    /* Structure to store circular buffer information */
    typedef struct {
        /* Audio data buffer */
        float* buffer;
        /* Buffer capacity */
        unsigned long capacity;
        /* Buffer's head */
        unsigned long head;
        /* Buffer's tail */
        unsigned long tail;
        /* Buffer size */
        unsigned long size;
    } CircularBuffer;

    /* Initialize a circular buffer */
    void CircularBuffer_Init(CircularBuffer* cb, unsigned long capacity);

    /* Frees the resources used by the circular buffer */
    void CircularBuffer_Free(CircularBuffer* cb);

    /* Writes data from the circular buffer */
    bool CircularBuffer_Write(CircularBuffer* cb, const float* data, unsigned long dataSize);

    /* Reads data from the circular buffer */
    bool CircularBuffer_Read(CircularBuffer* cb, float* data, unsigned long dataSize);

#ifdef __cplusplus
}
#endif

#endif //CIRCULAR_BUFFER_H
