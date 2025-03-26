//
// Created by maxim on 22/03/2025.
//

#ifndef VIDEO_STREAM_HPP
#define VIDEO_STREAM_HPP

extern "C"{
#include <stdint.h>

    /* Structure to store video device information */
    struct VideoDevice {
        /* Device index */
        int index;
        /* Device name */
        char name[256];
    };

    /* Captures a single frame from a video device */
    unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels, int* bufferSize);

    /* Frees the memory allocated for a frame */
    void FreeFrame(unsigned char* frame);

    /* Lists all available video devices */
    VideoDevice* GetVideoDevices(int * deviceCount);

    /* Frees the memory allocated for video devices */
    void FreeVideoDevices(VideoDevice* devices);

    /* Callback function for video frame */
    typedef void (*FrameCallback)(const uint8_t* data, int width, int height, int channels, void* userdata);

    /* Structure to store video stream configuration */
    typedef struct {
        /* Index of the video device */
        int device_index;
        /* Target FPS of the video stream */
        int target_fps;
        /* User data */
        void* user_data;
    } VideoStreamConfig;

    /* Creates a new video stream */
    void* VideoStreamCreate(const VideoStreamConfig* config);

    /* Starts the video stream */
    void VideoStreamStart(void* handle, FrameCallback callback);

    /* Stops the video stream */
    void VideoStreamStop(void* handle);

    /* Frees the memory allocated for the video stream handler*/
    void VideoStreamFree(void* handle);
}


#endif // VIDEO_STREAM_HPP
