//
// Created by maxim on 16/03/2025.
//

#include "circular_buffer.h"
#include <stdlib.h>


void CircularBuffer_Init(CircularBuffer* cb, unsigned long capacity) {
    cb->buffer = (float*)malloc(capacity * sizeof(float));
    cb->capacity = capacity;
    cb->head = 0;
    cb->tail = 0;
    cb->size = 0;
}

void CircularBuffer_Free(CircularBuffer* cb) {
    free(cb->buffer);
}

bool CircularBuffer_Write(CircularBuffer* cb, const float* data, unsigned long dataSize) {
    if (cb->size + dataSize > cb->capacity) {
        return false; // Buffer plein
    }

    for (int i = 0; i < dataSize; i++) {
        cb->buffer[cb->head] = data[i];
        cb->head = (cb->head + 1) % cb->capacity;
    }
    cb->size += dataSize;
    return true;
}

bool CircularBuffer_Read(CircularBuffer* cb, float* data, unsigned long dataSize) {
    if (cb->size < dataSize) {
        return false; // Pas assez de données
    }

    for (int i = 0; i < dataSize; i++) {
        data[i] = cb->buffer[cb->tail];
        cb->tail = (cb->tail + 1) % cb->capacity;
    }
    cb->size -= dataSize;
    return true;
}

