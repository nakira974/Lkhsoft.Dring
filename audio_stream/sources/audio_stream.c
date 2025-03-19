#include "audio_stream.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Buffer for error handling */
jmp_buf error_jmp_buf;

/* Callback function for audio capture */
static int recordAudioCallback(const void *inputBuffer, void *outputBuffer,
                        unsigned long framesPerBuffer,
                        const PaStreamCallbackTimeInfo *timeInfo,
                        const PaStreamCallbackFlags statusFlags,
                        void *userData) {
    AudioContext *context = (AudioContext *)userData;
    if (!context->isRunning) return paComplete;
    (void) outputBuffer;
    (void)timeInfo;
    (void)statusFlags;

    // Écrire les données audio dans le buffer circulaire
    if (inputBuffer) {
        CircularBuffer_Write(&context->circularBuffer, (const float*)inputBuffer, framesPerBuffer * context->numChannels);
    }

    // Si un callback managé est enregistré, l'appeler avec les données disponibles
    if (context->managedCallback) {
        float buffer[DEFAULT_BUFFER_SIZE]; // Buffer temporaire pour les données audio
        int samplesRead = CircularBuffer_Read(&context->circularBuffer, buffer, DEFAULT_BUFFER_SIZE);
        if (samplesRead > 0) {
            context->managedCallback(buffer, samplesRead);
        }
    }

    return paContinue;
}

/* Callback function for audio playback */
static int playAudioCallback(const void *inputBuffer, void *outputBuffer,
                         unsigned long framesPerBuffer,
                         const PaStreamCallbackTimeInfo *timeInfo,
                         const PaStreamCallbackFlags statusFlags,
                         void *userData) {
    AudioContext *context = (AudioContext *)userData;
    float *out = (float *)outputBuffer; // Pointeur pour écrire les données de sortie
    unsigned int i;

    (void)inputBuffer; // Prévenir les avertissements "unused variable"
    (void)timeInfo;
    (void)statusFlags;

    if (!context->isRunning) return paComplete;

    // Lire les données du buffer circulaire
    float tempBuffer[framesPerBuffer * context->numChannels];
    int samplesToRead = framesPerBuffer * context->numChannels;
    int samplesRead = CircularBuffer_Read(&context->circularBuffer, tempBuffer, samplesToRead);

    // Si le buffer circulaire est vide, générer un signal en dents de scie pur
    if (samplesRead < samplesToRead) {
        // Remplir le reste du buffer avec du silence ou un signal en dents de scie
        for (i = samplesRead; i < samplesToRead; i++) {
            tempBuffer[i] = 0.0f; // Silence ou générer un signal en dents de scie
        }
    }

    // Appliquer le signal en dents de scie aux données lues
    // https://portaudio.com/docs/v19-doxydocs/writing_a_callback.html
    for (i = 0; i < framesPerBuffer; i++) {
        // Canal gauche
        *out++ = tempBuffer[i * context->numChannels] * context->left_phase;

        // Canal droit (si stéréo)
        if (context->numChannels == 2) {
            *out++ = tempBuffer[i * context->numChannels + 1] * context->right_phase;
        }

        // Générer le signal en dents de scie
        context->left_phase += 0.01f;
        if (context->left_phase >= 1.0f) context->left_phase -= 2.0f;

        context->right_phase += 0.03f;
        if (context->right_phase >= 1.0f) context->right_phase -= 2.0f;
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
            THROW;
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
        const PaDeviceInfo* device_info = Pa_GetDeviceInfo(hostApiContext);
        context->left_phase = 0.0f;
        context->right_phase = 0.0f;
        context->sampleRate = sampleRate;
        context->numChannels = numChannels;
        context->isRunning = true;

        // Initialiser le buffer circulaire
        CircularBuffer_Init(&context->circularBuffer, bufferCapacity);

        // Ouvrir un flux audio avec le périphérique spécifié
        PaStreamParameters inputParameters;
        inputParameters.device = device_info->hostApi;
        inputParameters.channelCount = numChannels;
        inputParameters.sampleFormat = paFloat32;
        inputParameters.suggestedLatency = device_info->defaultLowInputLatency;
        inputParameters.hostApiSpecificStreamInfo = NULL;
        inputParameters.suggestedLatency = device_info->defaultLowOutputLatency;

        PaError err = Pa_OpenStream(
            &context->stream,
            &inputParameters,
            NULL,
            sampleRate,
            DEFAULT_BUFFER_SIZE,
            paClipOff,
            recordAudioCallback,
            context);

        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            THROW;
        }

        err = Pa_StartStream(context->stream);
        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            THROW;
        }

        return true;
    }
    CATCH {
        return false;
    }
    FINALLY;
}

bool Audio_StartPlay(AudioContext *context, int hostApiContext, int sampleRate, int numChannels, int bufferCapacity) {
    TRY {
        // Initialiser les phases du signal en dents de scie
        const PaDeviceInfo* device_info = Pa_GetDeviceInfo(hostApiContext);
        context->left_phase = 0.0f;
        context->right_phase = 0.0f;
        context->numChannels = numChannels;
        context->isRunning = true;

        CircularBuffer_Init(&context->circularBuffer, bufferCapacity);

        // Configurer les paramètres de sortie
        PaStreamParameters outputParameters;
        outputParameters.device = device_info->hostApi;
        outputParameters.channelCount = numChannels;
        outputParameters.sampleFormat = paFloat32;
        outputParameters.suggestedLatency =device_info->defaultLowOutputLatency;
        outputParameters.hostApiSpecificStreamInfo = NULL;

        // Ouvrir un flux audio en mode lecture
        PaError err = Pa_OpenStream(
            &context->stream,
            NULL,
            &outputParameters,
            sampleRate,
            DEFAULT_BUFFER_SIZE,
            paClipOff,
            playAudioCallback,
            context
        );

        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            THROW;
        }

        // Démarrer le flux audio
        err = Pa_StartStream(context->stream);
        if (err != paNoError) {
            fprintf(stderr, "PortAudio error: %s\n", Pa_GetErrorText(err));
            THROW;
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

void Audio_AddData(AudioContext *context, const float *data, int dataSize) {
    if (context->isRunning) {
        CircularBuffer_Write(&context->circularBuffer, data, dataSize);
    }
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
            THROW;
        }

        Device* devices = (Device*)malloc(numDevices * sizeof(Device));
        if (!devices) {
            fprintf(stderr, "Memory allocation failed\n");
            THROW;
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