cmake_minimum_required(VERSION 3.15)
project(audio_stream C)

set(CMAKE_C_STANDARD 99)

if(${CMAKE_BUILD_TYPE} STREQUAL "Debug")
    set(LKHSOFT_DRING_CLIENT_WINDOWS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Debug/net9.0-windows10.0.19041.0/win10-x64)
    set(LKHSOFT_DRING_CLIENT_ANDROID_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/..//Lkhsoft.Dring.Client/bin/Debug/net9.0-android)
    set(LKHSOFT_DRING_CLIENT_IOS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Debug/net9.0-ios/iossimulator-x64)
    set(LKHSOFT_DRING_CLIENT_MACOS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Debug/net9.0-maccatalyst/maccatalyst-x64)

else ()
    set(LKHSOFT_DRING_CLIENT_WINDOWS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Release/net9.0-windows10.0.19041.0)
    set(LKHSOFT_DRING_CLIENT_ANDROID_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Release/net9.0-android)
    set(LKHSOFT_DRING_CLIENT_IOS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Release/net9.0-ios)
    set(LKHSOFT_DRING_CLIENT_MACOS_BUILD_OUTPUT ${CMAKE_SOURCE_DIR}/../Lkhsoft.Dring.Client/bin/Release/net9.0-maccatalyst)
endif()


include(FetchContent)
# Télécharger et configurer portaudio
FetchContent_Declare(
        portaudio
        GIT_REPOSITORY https://github.com/PortAudio/portaudio.git
        GIT_TAG 35e35d4 # Specific commit
)

# Fetch and make PortAudio available
FetchContent_MakeAvailable(portaudio)

## OUTPUT LIBRARY CONFIGURATION PER COMPILER ###

set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})

if(${CMAKE_SYSTEM_NAME}  STREQUAL "Windows")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_WINDOWS_BUILD_OUTPUT})
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_WINDOWS_BUILD_OUTPUT}/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_WINDOWS_BUILD_OUTPUT}/lib)
elseif (${CMAKE_SYSTEM_NAME}  STREQUAL "Android")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_ANDROID_BUILD_OUTPUT})
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_ANDROID_BUILD_OUTPUT}/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_ANDROID_BUILD_OUTPUT}/lib)
elseif (${CMAKE_SYSTEM_NAME}  STREQUAL "iOS")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_IOS_BUILD_OUTPUT})
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_IOS_BUILD_OUTPUT}/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_IOS_BUILD_OUTPUT}/lib)
elseif (${CMAKE_SYSTEM_NAME}  STREQUAL "Darwin")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_MACOS_BUILD_OUTPUT})
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_MACOS_BUILD_OUTPUT}/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${LKHSOFT_DRING_CLIENT_MACOS_BUILD_OUTPUT}/lib)
else()
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/lib)
endif ()


# Fichiers sources et en-têtes
file(GLOB_RECURSE HEADERS_FILES
        ${PROJECT_SOURCE_DIR}/headers/*.h
        ${PROJECT_SOURCE_DIR}/headers/*.inl
)
file(GLOB_RECURSE SOURCES_FILES
        ${PROJECT_SOURCE_DIR}/sources/*.c
)

# Trouver et configurer les threads
find_package(Threads REQUIRED)
if (THREADS_HAVE_PTHREAD_ARG)
    target_compile_options(${PROJECT_NAME} PUBLIC "-pthread")
endif ()
if (CMAKE_THREAD_LIBS_INIT)
    target_link_libraries(${PROJECT_NAME} "${CMAKE_THREAD_LIBS_INIT}")
endif ()

# Inclure les répertoires d'en-têtes
include_directories(${PROJECT_SOURCE_DIR}/headers)


# Créer la bibliothèque
add_library(${CMAKE_PROJECT_NAME} SHARED ${SOURCES_FILES} ${HEADERS_FILES})

# Lier portaudio
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE portaudio)
target_include_directories(${CMAKE_PROJECT_NAME} PRIVATE ${portaudio_SOURCE_DIR}/include)


# Définir le répertoire de sortie pour PortAudio
set_target_properties(portaudio PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}
        ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_ARCHIVE_OUTPUT_DIRECTORY}
        LIBRARY_OUTPUT_DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}
)

message(STATUS "################# BUILD CONFIGURATION #################")
message(STATUS "Build type: ${CMAKE_BUILD_TYPE}")
message(STATUS "portaudio_SOURCE_DIR: ${portaudio_SOURCE_DIR}")
message(STATUS "portaudio_BINARY_DIR: ${portaudio_BINARY_DIR}")
message(STATUS "Output directories:")
message(STATUS "RUNTIME: ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")
message(STATUS "ARCHIVE: ${CMAKE_ARCHIVE_OUTPUT_DIRECTORY}")
message(STATUS "LIBRARY: ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}")