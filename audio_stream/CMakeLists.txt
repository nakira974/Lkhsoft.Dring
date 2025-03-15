cmake_minimum_required(VERSION 3.15)
project(audio_stream C)

set(CMAKE_C_STANDARD 99)

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
if (CMAKE_GENERATOR MATCHES "Visual Studio")
    set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/msbuild)
    set(CMAKE_COMPILE_PDB_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/msbuild/compiled)
    set(CMAKE_PDB_OUTPUT_DIRECTORY  ${CMAKE_SOURCE_DIR}/build/msbuild/pdb)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/msbuild/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/msbuild/lib)
elseif (CMAKE_GENERATOR MATCHES "MinGW Makefiles")
    set(CMAKE_SYSTEM_PROCESSOR x64)
    set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/lib)
elseif (CMAKE_SYSTEM_NAME MATCHES "Linux")
    if (CMAKE_C_COMPILER_ID STREQUAL "GNU")
        set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
        set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/gnu/)
        set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/gnu/archive)
        set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/gnu/lib)
    elseif (CMAKE_C_COMPILER_ID STREQUAL "Clang")
        set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
        set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/clang/)
        set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/clang/archive)
        set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/clang/lib)
    endif ()
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

message(STATUS "portaudio_SOURCE_DIR: ${portaudio_SOURCE_DIR}")
message(STATUS "portaudio_BINARY_DIR: ${portaudio_BINARY_DIR}")
message(STATUS "Output directories:")
message(STATUS "  - RUNTIME: ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}")
message(STATUS "  - ARCHIVE: ${CMAKE_ARCHIVE_OUTPUT_DIRECTORY}")
message(STATUS "  - LIBRARY: ${CMAKE_LIBRARY_OUTPUT_DIRECTORY}")