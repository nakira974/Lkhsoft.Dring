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
    /* Initialize portaudio */
    bool Audio_Initialize();

    /* Starts an audio recording */
    bool Audio_StartCapture(AudioContext *context, int sampleRate, int numChannels);

    /* Stops the audio recording */
    void Audio_StopCapture(AudioContext *context);

    /* Frees the resources used by portaudio */
    void Audio_Shutdown();
#ifdef __cplusplus
}
#endif

/* Global variable to handle errors */
extern jmp_buf error_jmp_buf;


#endif //AUDIO_STREAM_LIBRARY_H