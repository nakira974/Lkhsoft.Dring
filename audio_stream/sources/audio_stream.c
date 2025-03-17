#include "audio_stream.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Buffer for error handling */
jmp_buf error_jmp_buf;

/* Callback function for audio capture */
static int AudioCallback(const void *inputBuffer, void *outputBuffer,
                        unsigned long framesPerBuffer,
                        const PaStreamCallbackTimeInfo *timeInfo,
                        const PaStreamCallbackFlags statusFlags,
                        void *userData) {
    AudioContext *context = (AudioContext *)userData;
    if (!context->isRunning) return paComplete;

    // Écrire les données audio dans le buffer circulaire
    if (inputBuffer) {
        CircularBuffer_Write(&context->circularBuffer, (const float*)inputBuffer, framesPerBuffer * context->numChannels);
    }

    // Si un callback managé est enregistré, l'appeler avec les données disponibles
    if (context->managedCallback) {
        float buffer[1024]; // Buffer temporaire pour les données audio
        int samplesRead = CircularBuffer_Read(&context->circularBuffer, buffer, 1024);
        if (samplesRead > 0) {
            context->managedCallback(buffer, samplesRead);
        }
    }

    return paContinue;
}


void RegisterAudioDataCallback(AudioContext *context, AudioDataCallback callback) {
    context->managedCallback = callback;
}

bool Audio_Initialize() {
    TRY {
        PaError err = Pa_Initialize();
        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            longjmp(error_jmp_buf, 1);
        }
        return true;
    }
    CATCH {
        return false;
    }
    FINALLY;
}


bool Audio_StartCapture(AudioContext *context, int hostApiContext, int sampleRate, int numChannels, int bufferCapacity) {
    TRY {
        context->sampleRate = sampleRate;
        context->numChannels = numChannels;
        context->isRunning = true;

        // Initialiser le buffer circulaire
        CircularBuffer_Init(&context->circularBuffer, bufferCapacity);

        // Ouvrir un flux audio avec le périphérique spécifié
        PaStreamParameters inputParameters;
        inputParameters.device = hostApiContext;
        inputParameters.channelCount = numChannels;
        inputParameters.sampleFormat = paFloat32;
        inputParameters.suggestedLatency = Pa_GetDeviceInfo(hostApiContext)->defaultLowInputLatency;
        inputParameters.hostApiSpecificStreamInfo = NULL;

        PaError err = Pa_OpenStream(&context->stream, &inputParameters, NULL, sampleRate, 512, paClipOff, AudioCallback, context);
        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            longjmp(error_jmp_buf, 1);
        }

        err = Pa_StartStream(context->stream);
        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            longjmp(error_jmp_buf, 1);
        }

        return true;
    }
    CATCH {
        return false;
    }
    FINALLY;
}

int Audio_GetAudioData(AudioContext *context, float* buffer, int bufferSize) {
    if (!context->isRunning || !context->circularBuffer.buffer) {
        return 0; // Aucune donnée disponible
    }

    // Lire les données du buffer circulaire
    if (CircularBuffer_Read(&context->circularBuffer, buffer, bufferSize)) {
        return bufferSize;
    }
    return 0; // Pas assez de données disponibles
}

void Audio_StopCapture(AudioContext *context) {
    if (context->stream) {
        Pa_StopStream(context->stream);
        Pa_CloseStream(context->stream);
        context->stream = NULL;
    }
    context->isRunning = false;
}

void Audio_Shutdown(AudioContext *context) {
    if (context->stream) {
        Pa_StopStream(context->stream);
        Pa_CloseStream(context->stream);
        context->stream = NULL;
    }

    // free sur le buffer circulaire
    if (context->circularBuffer.buffer) {
        CircularBuffer_Free(&context->circularBuffer);
    }

    Pa_Terminate();
}

Device* GetAudioDevices(int* deviceCount) {
    TRY {
        // Obtenir le nombre de périphériques disponibles
        int numDevices = Pa_GetDeviceCount();
        if (numDevices < 0) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(numDevices));
            longjmp(error_jmp_buf, 1);
        }

        Device* devices = (Device*)malloc(numDevices * sizeof(Device));
        if (!devices) {
            fprintf(stderr, "Memory allocation failed\n");
            longjmp(error_jmp_buf, 1);
        }

        // Remplir le tableau avec les informations des périphériques
        for (int i = 0; i < numDevices; i++) {
            const PaDeviceInfo* deviceInfo = Pa_GetDeviceInfo(i);
            if (deviceInfo) {
                strncpy(devices[i].name, deviceInfo->name, sizeof(devices[i].name) - 1);
                devices[i].name[sizeof(devices[i].name) - 1] = '\0'; // Assurer la terminaison de la chaîne
                devices[i].hostApiIndex = deviceInfo->hostApi;
                devices[i].maxInputChannels = deviceInfo->maxInputChannels;
                devices[i].maxOutputChannels = deviceInfo->maxOutputChannels;
                devices[i].defaultSampleRate = deviceInfo->defaultSampleRate;
            }
        }

        // Retourner le tableau et le nombre de périphériques
        *deviceCount = numDevices;
        return devices;
    }
    CATCH {
        // En cas d'erreur, retourner NULL
        *deviceCount = 0;
        return NULL;
    }
    FINALLY;
}

void FreeAudioDevices(Device* devices) {
    free(devices);
}