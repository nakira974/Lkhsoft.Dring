#ifndef AUDIO_STREAM_LIBRARY_H
#define AUDIO_STREAM_LIBRARY_H

#include <portaudio.h>
#include <setjmp.h>
#include <stdbool.h>

// Définitions pour la gestion des erreurs
#define TRY do { if (setjmp(error_jmp_buf) == 0) {
#define CATCH } else {
#define FINALLY } } while (0);

/* Small structure to store audio process information */
typedef struct {
    PaStream *stream;
    int sampleRate;
    int numChannels;
    bool isRunning;
} AudioContext;

// Exported functions
#ifdef __cplusplus
extern "C" {
#endif
    /* Structure to store audio device information */
    typedef struct {
        /* Device information */
        char name[256];
        /* Maximum number of input channels */
        int maxInputChannels;
        /* Maximum number of output channels */
        int maxOutputChannels;
        /* Default sample rate */
        double defaultSampleRate;
    } Device;

    /* Initialize portaudio */
    bool Audio_Initialize();

    /* Starts an audio recording */
    bool Audio_StartCapture(AudioContext *context, int sampleRate, int numChannels);

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