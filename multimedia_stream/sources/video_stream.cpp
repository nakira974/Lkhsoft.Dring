//
// Created by maxim on 22/03/2025.
//
#include "video_stream.h"

#include <opencv2/videoio.hpp>
#include <opencv2/imgproc.hpp>
#include <iostream>
#include "utils/exception.h"

#ifdef _WIN32
#include <windows.h>
#include <dshow.h>
#include <comdef.h>
#elif defined(__linux__)
#include <linux/videodev2.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/ioctl.h>
#elif defined(__ANDROID__)
// En-têtes spécifiques à Android
#elif defined(__APPLE__)
#include <TargetConditionals.h>
#if TARGET_OS_IPHONE
// En-têtes spécifiques à iOS
#elif TARGET_OS_MAC
// En-têtes spécifiques à macOS
#endif
#endif


unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels) {
    TRY{
        // Ouvrir la capture vidéo
        cv::VideoCapture cap(deviceIndex);
        if (!cap.isOpened()) {
            std::cerr << "Erreur : Impossible d'ouvrir la caméra." << std::endl;
            THROW;
        }

        // Capturer une frame
        cv::Mat frame;
        cap >> frame;
        if (frame.empty()) {
            std::cerr << "Erreur : Impossible de capturer une image." << std::endl;
            // Échec de la capture
           THROW;
        }

        // Récupérer les dimensions de l'image
        *width = frame.cols;
        *height = frame.rows;
        *channels = frame.channels();

        // Convertir l'image en format BGR
        cv::Mat bmpFrame;
        cv::cvtColor(frame, bmpFrame, cv::COLOR_BGR2RGB);

        // Allouer un buffer pour stocker les données de l'image
        int bufferSize = bmpFrame.total() * bmpFrame.elemSize();
        unsigned char* result = new unsigned char[bufferSize];

        // Copier les données de l'image dans le buffer
        std::memcpy(result, bmpFrame.data, bufferSize);

        return result; // Retourner les données de l'image
    }CATCH{
        std::cerr << "Erreur : Capture vidéo impossible" << std::endl;
        return nullptr;
    }
    FINALLY;
}

void FreeFrame(unsigned char* frame) {
   free(frame);
}
/* Converts a COM string to a std string */
std::string ConvertBSTRToStdString(BSTR bstr) {
    if (!bstr) return "";

    // Calculer la longueur de la chaîne UTF-8 résultante (sans le caractère nul de fin)
    int length = WideCharToMultiByte(CP_UTF8, 0, bstr, -1, nullptr, 0, nullptr, nullptr);
    if (length == 0) {
        // Échec de la conversion
        return "";
    }

    // Allouer un buffer pour la chaîne UTF-8
    std::string result(length - 1, 0); // On ignore le caractère nul de fin

    // Convertir la chaîne BSTR en UTF-8
    if (WideCharToMultiByte(CP_UTF8, 0, bstr, -1, &result[0], length, nullptr, nullptr) == 0) {
        // Échec de la conversion
        return "";
    }

    return result;
}

/* Lookup on the host for physical devices then video devices and returns their names */
std::vector<std::string> GetDeviceNames() {
    std::vector<std::string> deviceNames;

#ifdef _WIN32
 APTTYPE aptType;
    APTTYPEQUALIFIER aptQualifier;
    HRESULT hr = CoGetApartmentType(&aptType, &aptQualifier);
    if (hr == CO_E_NOTINITIALIZED) {
        // COM n'est pas initialisé, on l'initialise en mode multithread
        hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
        if (FAILED(hr)) {
            std::cerr << "Échec de l'initialisation de COM : " << std::hex << hr << std::endl;
            return deviceNames;
        }
    } else if (FAILED(hr)) {
        std::cerr << "Échec de CoGetApartmentType : " << std::hex << hr << std::endl;
        return deviceNames;
    }

    // Créer l'énumérateur de périphériques
    ICreateDevEnum* pDevEnum = nullptr;
    hr = CoCreateInstance(CLSID_SystemDeviceEnum, nullptr, CLSCTX_INPROC_SERVER,
                          IID_PPV_ARGS(&pDevEnum));
    if (FAILED(hr)) {
        std::cerr << "Échec de CoCreateInstance : " << std::hex << hr << std::endl;
        if (hr != RPC_E_CHANGED_MODE) {
            CoUninitialize();
        }
        return deviceNames;
    }

    // Énumérer les périphériques vidéo
    IEnumMoniker* pEnum = nullptr;
    hr = pDevEnum->CreateClassEnumerator(CLSID_VideoInputDeviceCategory, &pEnum, 0);
    if (FAILED(hr)) {
        std::cerr << "Échec de CreateClassEnumerator : " << std::hex << hr << std::endl;
        pDevEnum->Release();
        if (hr != RPC_E_CHANGED_MODE) {
            CoUninitialize();
        }
        return deviceNames;
    }

    // Parcourir les périphériques
    IMoniker* pMoniker = nullptr;
    while (pEnum->Next(1, &pMoniker, nullptr) == S_OK) {
        IPropertyBag* pPropBag = nullptr;
        hr = pMoniker->BindToStorage(nullptr, nullptr, IID_PPV_ARGS(&pPropBag));
        if (SUCCEEDED(hr)) {
            VARIANT var;
            VariantInit(&var);
            hr = pPropBag->Read(L"FriendlyName", &var, 0);
            if (SUCCEEDED(hr)) {
                deviceNames.push_back(ConvertBSTRToStdString(var.bstrVal));
                VariantClear(&var);
            }
            pPropBag->Release();
        }
        pMoniker->Release();
    }

    // Libérer les ressources
    pEnum->Release();
    pDevEnum->Release();

    // Ne pas appeler CoUninitialize si COM était déjà initialisé
    if (hr != RPC_E_CHANGED_MODE) {
        CoUninitialize();
    }

#elif defined(__linux__)
    const std::string basePath = "/dev/video";

    for (int i = 0; i < 10; ++i) {
        std::string devicePath = basePath + std::to_string(i);
        int fd = open(devicePath.c_str(), O_RDONLY);
        if (fd == -1) {
            break;
        }

        v4l2_capability cap;
        if (ioctl(fd, VIDIOC_QUERYCAP, &cap) == 0) {
            deviceNames.push_back(reinterpret_cast<const char*>(cap.card));
        }
        close(fd);
    }
#elif defined(__ANDROID__)
    // Implémenter la logique spécifique à Android
#elif defined(__APPLE__)
#if TARGET_OS_IPHONE
    // Implémenter la logique spécifique à iOS
#elif TARGET_OS_MAC
    // Implémenter la logique spécifique à macOS
#endif
#endif

    return deviceNames;
}


VideoDevice* GetVideoDevices(int* deviceCount) {
    std::vector<VideoDevice> devices;
    std::vector<std::string> deviceNames = GetDeviceNames();

    constexpr int maxDevices = 10;
    TRY
    for (int i = 0; i < maxDevices; i++) {
        cv::VideoCapture cap(i);
        if (cap.isOpened()) {
            VideoDevice device;
            device.index = i;
            if (i < deviceNames.size()) {
                device.name = deviceNames[i];
            } else {
                device.name = "Device " + std::to_string(i);
            }
            devices.push_back(device);
            cap.release();
        } else {
            break;
        }
    }

    *deviceCount = static_cast<int>(devices.size());
    auto* result = static_cast<VideoDevice*>(malloc(*deviceCount * sizeof(VideoDevice)));

    if (!result) {
        fprintf(stderr, "Memory allocation failed\n");
        THROW;
    }
    return result;
    CATCH
    fprintf(stderr, "Error while enumerating video devices\n");
    devices.clear();
    return nullptr;
    FINALLY
}

void FreeVideoDevices(VideoDevice* devices) {
    free(devices);
}