#ifndef AUDIO_STREAM_LIBRARY_H
#define AUDIO_STREAM_LIBRARY_H

#include "circular_buffer.h"
#include <portaudio.h>
#include <setjmp.h>
#include <stdbool.h>

// Définitions pour la gestion des erreurs
#define TRY do { if (setjmp(error_jmp_buf) == 0) {
#define CATCH } else {
#define FINALLY } } while (0);


// Exported functions
#ifdef __cplusplus
extern "C" {
#endif
    /* Definition of an audio data callback */
    typedef void (*AudioDataCallback)(const float* data, int size);

    /* Small structure to store audio process information */
    typedef struct {
        /* Audio stream */
        PaStream *stream;
        /* Sample rate */
        int sampleRate;
        /* Number of channels */
        int numChannels;
        /* Flag to indicate if the audio is running */
        bool isRunning;
        /* Audio circular buffer */
        CircularBuffer circularBuffer;
        /* Callback function */
        AudioDataCallback managedCallback;
    } AudioContext;

    /* Structure to store audio device information */
    typedef struct {
        /* Index of the device */
        int hostApiIndex;
        /* Device information */
        char name[256];
        /* Maximum number of input channels */
        int maxInputChannels;
        /* Maximum number of output channels */
        int maxOutputChannels;
        /* Default sample rate */
        double defaultSampleRate;
    } Device;

    /* Registers the audio callback */
    void RegisterAudioDataCallback(AudioContext *context, AudioDataCallback callback);

    /* Initialize portaudio */
    bool Audio_Initialize();

    /* Starts an audio recording */
    bool Audio_StartCapture(AudioContext *context, int hostApiContext, int sampleRate, int numChannels, int bufferCapacity);

    /* Get the audio data */
    int Audio_GetAudioData(AudioContext *context, float* buffer, int bufferSize);

    /* Stops the audio recording */
    void Audio_StopCapture(AudioContext *context);

    /* Frees the resources used by portaudio */
    void Audio_Shutdown();

    /* Get the list of audio devices */
    Device* GetAudioDevices(int* deviceCount);

    /* Frees the devices struct array*/
    void FreeAudioDevices(Device* devices);
#ifdef __cplusplus
}
#endif

/* Global variable to handle errors */
extern jmp_buf error_jmp_buf;


#endif //AUDIO_STREAM_LIBRARY_H