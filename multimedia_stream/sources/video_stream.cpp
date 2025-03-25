//
// Created by maxim on 22/03/2025.
//
#include "video_stream.h"

#include <opencv2/videoio.hpp>
#include <opencv2/imgproc.hpp>
#include <iostream>
#include "utils/exception.h"
#include "opencv2/imgcodecs.hpp"

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


unsigned char* CaptureFrame(int deviceIndex, int* width, int* height, int* channels, int* bufferSize) {
    try {
        cv::VideoCapture cap(deviceIndex);
        if (!cap.isOpened()) {
            std::cerr << "Erreur : Impossible d'ouvrir la caméra." << std::endl;
            return nullptr;
        }

        cv::Mat frame;
        cap >> frame;
        if (frame.empty()) {
            std::cerr << "Erreur : Impossible de capturer une image." << std::endl;
            return nullptr;
        }

        *width = frame.cols;
        *height = frame.rows;
        *channels = 3; // JPEG sera toujours 3 canaux (RGB)

        // Conversion en JPEG
        std::vector<unsigned char> jpegBuffer;
        cv::imencode(".jpg", frame, jpegBuffer, {
            cv::IMWRITE_JPEG_QUALITY, 80,        // Qualité (0-100)
            cv::IMWRITE_JPEG_OPTIMIZE, 1,        // Optimisation
            cv::IMWRITE_JPEG_PROGRESSIVE, 1      // JPEG progressif
        });

        // Allocation du buffer résultat
        unsigned char* result = new unsigned char[jpegBuffer.size()];
        std::memcpy(result, jpegBuffer.data(), jpegBuffer.size());
        *bufferSize = static_cast<int>(jpegBuffer.size());
        return result;
    }
    catch (const cv::Exception& e) {
        std::cerr << "Erreur OpenCV: " << e.what() << std::endl;
        return nullptr;
    }
    catch (...) {
        std::cerr << "Erreur inconnue lors de la capture" << std::endl;
        return nullptr;
    }
}

void FreeFrame(unsigned char* frame) {
   free(frame);
}

#ifdef WIN32
/* Converts a COM string to a std string */
char* WINAPI ConvertBSTRToString(BSTR pSrc)
{
    DWORD cb, cwch;
    char *szOut = NULL;

    if (!pSrc) return NULL;

    /* Retrieve the size of the BSTR with the NULL terminator */
    cwch = ::SysStringLen(pSrc) + 1;

    /* Compute the needed size with the NULL terminator */
    cb = ::WideCharToMultiByte(CP_ACP, 0, pSrc, cwch, NULL, 0, NULL, NULL);
    if (cb == 0)
    {
        cwch = ::GetLastError();
        ::_com_issue_error(!IS_ERROR(cwch) ? HRESULT_FROM_WIN32(cwch) : cwch);
        return NULL;
    }

    /* Allocate the string */
    szOut = (char*)::operator new(cb * sizeof(char));
    if (!szOut)
    {
        ::_com_issue_error(HRESULT_FROM_WIN32(ERROR_OUTOFMEMORY));
        return NULL;
    }

    /* Convert the string and NULL-terminate */
    szOut[cb - 1] = '\0';
    if (::WideCharToMultiByte(CP_ACP, 0, pSrc, cwch, szOut, cb, NULL, NULL) == 0)
    {
        /* We failed, clean everything up */
        cwch = ::GetLastError();

        ::operator delete(szOut);
        szOut = NULL;

        ::_com_issue_error(!IS_ERROR(cwch) ? HRESULT_FROM_WIN32(cwch) : cwch);
    }

    return szOut;
}
#endif
/* Lookup on the host for physical devices then video devices and returns their names */
char** GetDeviceNames(int* count) {
    *count = 0;
    char** deviceNames = nullptr;
    std::vector<std::string> tempNames;

#ifdef _WIN32
    APTTYPE aptType;
    APTTYPEQUALIFIER aptQualifier;
    HRESULT hr = CoGetApartmentType(&aptType, &aptQualifier);
    if (hr == CO_E_NOTINITIALIZED) {
        hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
        if (FAILED(hr)) {
            std::cerr << "Échec de l'initialisation de COM : " << std::hex << hr << std::endl;
            return nullptr;
        }
    } else if (FAILED(hr)) {
        std::cerr << "Échec de CoGetApartmentType : " << std::hex << hr << std::endl;
        return nullptr;
    }

    ICreateDevEnum* pDevEnum = nullptr;
    hr = CoCreateInstance(CLSID_SystemDeviceEnum, nullptr, CLSCTX_INPROC_SERVER,
                        IID_PPV_ARGS(&pDevEnum));
    if (FAILED(hr)) {
        std::cerr << "Échec de CoCreateInstance : " << std::hex << hr << std::endl;
        if (hr != RPC_E_CHANGED_MODE) {
            CoUninitialize();
        }
        return nullptr;
    }

    IEnumMoniker* pEnum = nullptr;
    hr = pDevEnum->CreateClassEnumerator(CLSID_VideoInputDeviceCategory, &pEnum, 0);
    if (FAILED(hr)) {
        std::cerr << "Échec de CreateClassEnumerator : " << std::hex << hr << std::endl;
        pDevEnum->Release();
        if (hr != RPC_E_CHANGED_MODE) {
            CoUninitialize();
        }
        return nullptr;
    }

    IMoniker* pMoniker = nullptr;
    while (pEnum->Next(1, &pMoniker, nullptr) == S_OK) {
        IPropertyBag* pPropBag = nullptr;
        hr = pMoniker->BindToStorage(nullptr, nullptr, IID_PPV_ARGS(&pPropBag));
        if (SUCCEEDED(hr)) {
            VARIANT var;
            VariantInit(&var);
            hr = pPropBag->Read(L"FriendlyName", &var, 0);
            if (SUCCEEDED(hr)) {
                tempNames.push_back(ConvertBSTRToString(var.bstrVal));
                VariantClear(&var);
            }
            pPropBag->Release();
        }
        pMoniker->Release();
    }

    pEnum->Release();
    pDevEnum->Release();

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
            tempNames.push_back(reinterpret_cast<const char*>(cap.card));
        }
        close(fd);
    }
#endif

    // Convertir le vector en tableau de char*
    if (!tempNames.empty()) {
        deviceNames = new char*[tempNames.size()];
        *count = tempNames.size();

        for (size_t i = 0; i < tempNames.size(); ++i) {
            deviceNames[i] = new char[tempNames[i].size() + 1];
            strcpy(deviceNames[i], tempNames[i].c_str());
        }
    }

    return deviceNames;
}



VideoDevice* GetVideoDevices(int* deviceCount) {
    std::vector<VideoDevice> devices;
    auto devicesCount = static_cast<int *>(malloc(1 * sizeof(int)));
    char** deviceNames = GetDeviceNames(devicesCount);
    if (deviceCount == nullptr || deviceNames == nullptr) {
        return nullptr;
    }

    constexpr int maxDevices = 10;
    TRY
    for (int i = 0; i < maxDevices; i++) {
        cv::VideoCapture cap(i);
        if (cap.isOpened()) {
            VideoDevice device;
            device.index = i;
            if (i < *devicesCount) {
                strncpy(device.name, deviceNames[i], sizeof(device.name) - 1);
            } else {
                strncpy(device.name, std::to_string(i).c_str(), sizeof(device.name) - 1);
            }
            devices.push_back(device);
            cap.release();
        } else {
            break;
        }
    }

    *deviceCount = static_cast<int>(devices.size());
    auto* result = static_cast<VideoDevice*>(malloc(*deviceCount * sizeof(VideoDevice)));
    for (int i = 0; i < *deviceCount; i++) {
        result[i] = devices[i];
    }

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