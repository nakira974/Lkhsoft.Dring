//
// Created by maxim on 22/03/2025.
//
#include "video_stream.h"
#include "opencv2/videoio/videoio_c.h"
#include <opencv2/imgproc/imgproc_c.h>
#include <stdio.h>
#include <stdlib.h>

unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels) {
    // Ouvrir la capture vidéo
    CvCapture* cap = cvCreateCameraCapture(deviceIndex);
    if (!cap) {
        return NULL; // Échec de l'ouverture de la capture
    }

    // Capturer une frame
    IplImage* frame = cvQueryFrame(cap);
    if (!frame) {
        cvReleaseCapture(&cap);
        return NULL; // Échec de la capture
    }

    // Récupérer les dimensions de l'image
    *width = frame->width;
    *height = frame->height;
    *channels = frame->nChannels;

    // Convertir l'image en format BMP
    IplImage* bmpFrame = cvCreateImage(cvGetSize(frame), IPL_DEPTH_8U, 3);
    if (!bmpFrame) {
        cvReleaseCapture(&cap);
        return NULL; // Échec de la création de l'image BMP
    }

    // Copier et convertir l'image en BGR (format attendu par BMP)
    cvCvtColor(frame, bmpFrame, CV_BGR2RGB);

    // Allouer un buffer pour stocker les données de l'image
    int bufferSize = bmpFrame->imageSize;
    unsigned char* result = (unsigned char*)malloc(bufferSize);
    if (!result) {
        cvReleaseImage(&bmpFrame);
        cvReleaseCapture(&cap);
        return NULL; // Échec de l'allocation mémoire
    }

    // Copier les données de l'image dans le buffer
    memcpy(result, bmpFrame->imageData, bufferSize);

    // Libérer les ressources
    cvReleaseImage(&bmpFrame);
    cvReleaseCapture(&cap);

    return result; // Retourner les données de l'image
}

VideoDevice* GetVideoDevices(int* deviceCount) {
    const int maxDevices = 10; // Nombre maximal de périphériques à tester
    VideoDevice* devices = (VideoDevice*)malloc(maxDevices * sizeof(VideoDevice));
    if (!devices) {
        *deviceCount = 0;
        return NULL; // Échec de l'allocation mémoire
    }

    int count = 0;

    // Tester les indices jusqu'à trouver tous les périphériques
    for (int i = 0; i < maxDevices; i++) {
        CvCapture* cap = cvCreateCameraCapture(i);
        if (cap) {
            devices[count].index = i;
            snprintf(devices[count].name, sizeof(devices[count].name), "Device %d", i);
            count++;
            cvReleaseCapture(&cap);
        } else {
            break; // Arrêter si aucun périphérique n'est trouvé
        }
    }

    *deviceCount = count;
    return devices;
}

void FreeFrame(unsigned char* frame) {
    free(frame);
}