#include "audio_stream.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// Buffer pour la gestion des erreurs
jmp_buf error_jmp_buf;

/* Callback function for audio capture */
static int AudioCallback(const void *inputBuffer, void *outputBuffer,
                        unsigned long framesPerBuffer,
                        const PaStreamCallbackTimeInfo *timeInfo,
                        PaStreamCallbackFlags statusFlags,
                        void *userData) {
    AudioContext *context = (AudioContext *)userData;
    if (!context->isRunning) return paComplete;

    //TODO Traiter les données audio (inputBuffer) ici
    // Transférer vers un périphérique ou les enregistrer... à voir

    return paContinue;
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

bool Audio_StartCapture(AudioContext *context, int sampleRate, int numChannels) {
    TRY {
        context->sampleRate = sampleRate;
        context->numChannels = numChannels;
        context->isRunning = true;

        PaError err = Pa_OpenDefaultStream(&context->stream, numChannels, 0, paFloat32, sampleRate, 512, AudioCallback, context);
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

void Audio_StopCapture(AudioContext *context) {
    if (context->stream) {
        Pa_StopStream(context->stream);
        Pa_CloseStream(context->stream);
        context->stream = NULL;
    }
    context->isRunning = false;
}

void Audio_Shutdown() {
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