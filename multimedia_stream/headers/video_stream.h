//
// Created by maxim on 22/03/2025.
//

#ifndef VIDEO_STREAM_H
#define VIDEO_STREAM_H
#include "utils/exception.h"

#ifdef __cplusplus
extern "C" {
#endif
    /* Video device structure */
    typedef struct {
        /* Index of the device */
        int index;
        /* Device name */
        char name[256];
    } VideoDevice;

    /* Capture a frame from a video device */
    unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels);

    /* Free the frame buffer */
    void FreeFrame(unsigned char* frame);

    /* Get the list of available video devices */
    VideoDevice* GetVideoDevices(int* deviceCount);

#ifdef __cplusplus
}
#endif
#endif //VIDEO_STREAM_H
