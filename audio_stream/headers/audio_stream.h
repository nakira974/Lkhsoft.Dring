#ifndef AUDIO_STREAM_LIBRARY_H
#define AUDIO_STREAM_LIBRARY_H

#include "circular_buffer.h"
#include <portaudio.h>
#include <setjmp.h>
#include <stdbool.h>

/* ######### ERROR HANDLING MACROS  #########*/

/* Try instruction  */
#define TRY do { if (setjmp(error_jmp_buf) == 0) {
/* Catch instruction */
#define CATCH } else {
/* Finally instruction */
#define FINALLY } } while (0);
/* Throw instruction */
#define THROW longjmp(error_jmp_buf, 1)

/* ######### AUDIO STREAM DEFINITIONS #########*/

/* Default buffer size */
#define DEFAULT_BUFFER_SIZE 1024



// Exported functions
#ifdef __cplusplus
extern "C" {
#endif
    /* Definition of an audio data callback */
    typedef void (*AudioDataCallback)(const float* data, int size);

    /* Small structure to store audio process information */
    typedef struct {
        /* Left phase of the signal */
        float left_phase;
        /* Right phase of the signal */
        float right_phase;
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
        /* Index of the API*/
        int hostApiIndex;
        /* Index of the device in the host API */
        int hostApiDeviceIndex;
        /* Type of the host API */
        PaHostApiTypeId hostApiType;
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
    bool Audio_StartCapture(AudioContext *context, int hostApiDeviceIndex, int sampleRate, int numChannels, int bufferCapacity);

    /* Starts an audio playback */
    bool Audio_StartPlay(AudioContext *context, int hostApiDeviceIndex, int sampleRate, int numChannels, int bufferCapacity);

    /* Writes data to the circular buffer to be fetched by the callback */
    void Audio_AddData(AudioContext *context, const float *data, int dataSize);

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