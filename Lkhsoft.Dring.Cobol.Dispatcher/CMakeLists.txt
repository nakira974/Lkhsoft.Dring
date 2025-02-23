cmake_minimum_required(VERSION 3.25.1)
project(transmitter C)

set(CMAKE_C_STANDARD 99)

file(GLOB_RECURSE HEADERS_FILES
        ${PROJECT_SOURCE_DIR}/headers/*.h
        ${PROJECT_SOURCE_DIR}/headers/*.inl
)
file(GLOB_RECURSE SOURCES_FILES
        ${PROJECT_SOURCE_DIR}/src/*.c
)

if (CMAKE_SYSTEM_NAME MATCHES "Windows")
    find_package(Threads REQUIRED)
    if (THREADS_HAVE_PTHREAD_ARG)
        target_compile_options(${PROJECT_NAME} PUBLIC "-pthread")
    endif ()
    if (CMAKE_THREAD_LIBS_INIT)
        target_link_libraries(${PROJECT_NAME} "${CMAKE_THREAD_LIBS_INIT}")
    endif ()
endif ()

set(CMAKE_C_STANDARD 99)
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
    #set(CMAKE_COMPILE_PDB_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/compiled)
    #set(CMAKE_PDB_OUTPUT_DIRECTORY  ${CMAKE_SOURCE_DIR}/build/mingw32_x86/pdb)
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/archive)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/build/mingw32_x86/lib)
    if (CMAKE_SYSTEM_NAME MATCHES "Linux")
        if (CMAKE_C_COMPILER_ID STREQUAL "GNU")
            set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
            set(CMAKE_RUNTIME_OUTPUT_DIRECTORY /usr/local/bin)
            #set(CMAKE_COMPILE_PDB_OUTPUT_DIRECTORY /usr/local/bin/compiled)
            #set(CMAKE_PDB_OUTPUT_DIRECTORY  /usr/local/bin/pdb)
            set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY /usr/local/lib/archive)
            set(CMAKE_LIBRARY_OUTPUT_DIRECTORY /usr/local/lib)
        elseif (CMAKE_C_COMPILER_ID STREQUAL "Clang")
            set(CMAKE_ROOT_DIRECTORY ${CMAKE_SOURCE_DIR})
            set(CMAKE_RUNTIME_OUTPUT_DIRECTORY /usr/local/bin)
            #set(CMAKE_COMPILE_PDB_OUTPUT_DIRECTORY /usr/local/bin/compiled)
            #set(CMAKE_PDB_OUTPUT_DIRECTORY  /usr/local/bin/pdb)
            set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY /usr/local/lib/archive)
            set(CMAKE_LIBRARY_OUTPUT_DIRECTORY /usr/local/lib)
        endif ()
    endif ()
endif ()

include_directories(${PROJECT_SOURCE_DIR}/headers)

add_executable(${PROJECT_NAME} ${SOURCES_FILES} ${HEADERS_FILES})

target_link_libraries(${PROJECT_NAME})

if(WIN32)
    target_link_libraries(${PROJECT_NAME} wsock32 ws2_32)
endif()
