#include "audio_stream.h"
#include <stdio.h>
#include <stdlib.h>

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