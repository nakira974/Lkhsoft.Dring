//
// Created by maxim on 22/03/2025.
//

#ifndef VIDEO_STREAM_HPP
#define VIDEO_STREAM_HPP
#include <string>

extern "C"{
    /* Structure to store video device information */
    struct VideoDevice {
        /* Device index */
        int index;
        /* Device name */
        std::string name;
    };

    /* Captures a frame from a video device */
    unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels);

    /* Frees the memory allocated for a frame */
    void FreeFrame(unsigned char* frame);

    /* Lists all available video devices */
    VideoDevice* GetVideoDevices(int * deviceCount);

    /* Frees the memory allocated for video devices */
    void FreeVideoDevices(VideoDevice* devices);
}


#endif // VIDEO_STREAM_HPP
