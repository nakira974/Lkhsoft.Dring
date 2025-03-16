using System.Reflection;
using Lkhsoft.Dring.Client.Exceptions;

namespace Lkhsoft.Dring.Client.Services;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Audio service implementation
/// </summary>
public class AudioService : IAudioService
{
    /// <summary>
    /// Audio context from native library
    /// </summary>
    private IntPtr _audioContext;
    
    /// <summary>
    /// Audio data callback
    /// </summary>
    private IAudioService.AudioDataCallback _audioDataCallback;
    
    /// <summary>
    /// Native library name
    /// </summary>
    private const string nativeLibraryName = "libaudio_stream";
    
    /// <summary>
    /// Selected audio device
    /// </summary>
    public Device SelectedDevice { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <exception cref="Exception">The native library failed to initialize</exception>
    /// <exception cref="DllNotFoundException">Could not find audio_stream library the assembly</exception>
    public AudioService()
    {
        LoadNativeLibrary();
        // malloc the audio context
        _audioContext = Marshal.AllocHGlobal(Marshal.SizeOf<AudioContext>());
        if (!Audio_Initialize())
        {
            throw new Exception("Failed to initialize audio library");
        }
    }

    /// <inheritdoc/>
    public void StartCapture(int hostApiIndex, int sampleRate, int numChannels, int bufferCapacity)
    {
        if (!Audio_StartCapture(_audioContext, hostApiIndex, sampleRate, numChannels, bufferCapacity))
        {
            throw new Exception("Failed to start audio capture.");
        }
    }
    
    /// <inheritdoc/>
    public float[] GetAudioData(int bufferSize)
    {
        float[] buffer = new float[bufferSize];
        int samplesRead = Audio_GetAudioData(_audioContext, buffer, bufferSize);
        if (samplesRead == 0)
        {
            throw new Exception("No audio data available.");
        }
        return buffer;
    }
    
    public void RegisterCallback(IAudioService.AudioDataCallback callback)
    {
        _audioDataCallback = callback; // Garder une référence pour éviter le GC
        IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        Marshal.WriteIntPtr(_audioContext + Marshal.OffsetOf<AudioContext>("ManagedCallback"), callbackPtr);
        RegisterAudioDataCallback(_audioContext, callback);
    }

    /// <inheritdoc/>
    public void StopCapture()
    {
        Audio_StopCapture(_audioContext);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Audio_Shutdown();
        Marshal.FreeHGlobal(_audioContext);
        GC.SuppressFinalize(this);
    }
    
    /// <inheritdoc/>
    public Device[] GetAudioDevices()
    {
        var devicesPtr = GetAudioDevices(out var deviceCount);

        if (devicesPtr == IntPtr.Zero || deviceCount == 0)
        {
            throw new Exception("Failed to list audio devices");
        }

        // Convertir le tableau natif en tableau C#
        var devices = new Device[deviceCount];
        for (var i = 0; i < deviceCount; i++)
        {
            // memcpy(&devices[i], devicesPtr + i * sizeof(Device), sizeof(Device));
            devices[i] = Marshal.PtrToStructure<Device>(devicesPtr + i * Marshal.SizeOf<Device>());
        }

        // free du pointeur alloué par la fonction native
        FreeAudioDevices(devicesPtr);
        return devices;
    }
    #region PRIVATE METHODS
    /// <summary>
    /// Loads the native library
    /// </summary>
    /// <exception cref="DllNotFoundException">The native library could not be found in the assembly output</exception>
    private static void LoadNativeLibrary()
    {
        try
        {
            // Charger la bibliothèque native
            NativeLibrary.Load(nativeLibraryName, Assembly.GetCallingAssembly(), DllImportSearchPath.AssemblyDirectory );
        }
        catch (Exception ex)
        {
            throw new DllNotFoundException($"Failed to load native library '{nativeLibraryName}'", ex);
        }
    }
    #endregion
    
    #region NATIVE METHODS
    /// <summary>
    /// Initializes the audio library
    /// </summary>
    /// <returns>True if PortAudio has been correctly initialized, otherwise false</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Audio_Initialize();

    /// <summary>
    /// Starts capturing audio
    /// </summary>
    /// <param name="context">PortAudio context</param>
    /// <param name="hostApiContext">Device index</param>
    /// <param name="sampleRate">Capture sample rate</param>
    /// <param name="numChannels">Number of channels</param>
    /// <param name="bufferCapacity">Buffer capacity</param>
    /// <returns>True if the capture has been correctly initialized, otherwise false</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Audio_StartCapture(IntPtr context, int hostApiContext, int sampleRate, int numChannels, int bufferCapacity);
    /// <summary>
    /// Stops capturing audio
    /// </summary>
    /// <param name="context">PortAudio context to be destroyed</param>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Audio_StopCapture(IntPtr context);

    /// <summary>
    /// Shuts down the audio library
    /// </summary>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Audio_Shutdown();
    
    /// <summary>
    /// Gets the audio devices available on the system
    /// </summary>
    /// <param name="deviceCount">Number of available devices on the system</param>
    /// <returns>A pointer to malloc allocated Device struct array</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetAudioDevices(out int deviceCount);
    
    /// <summary>
    /// Frees the audio devices array
    /// </summary>
    /// <param name="devices">Device struct array to desallocated</param>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeAudioDevices(IntPtr devices);
    
    /// <summary>
    /// Gets the audio data from the audio stream
    /// </summary>
    /// <param name="context">Audio context</param>
    /// <param name="bufferSize">Buffer size</param>
    /// <returns>A pointer to the audio data</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Audio_GetAudioData(IntPtr context, float[] buffer, int bufferSize);
    
    /// <summary>
    /// Registers the audio data callback
    /// </summary>
    /// <param name="context"></param>
    /// <param name="callback"></param>
    [DllImport("audio_stream", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RegisterAudioDataCallback(IntPtr context, IAudioService.AudioDataCallback callback);
    #endregion
}

/// <summary>
/// Audio device structure
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public record struct Device
{
    /// <summary>
    /// Index of the device
    /// </summary>
    public int HostApiIndex;
    /// <summary>
    /// Device name
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Name;
    /// <summary>
    /// Maximum input channels
    /// </summary>
    public int MaxInputChannels;
    /// <summary>
    /// Maximum output channels
    /// </summary>
    public int MaxOutputChannels;
    /// <summary>
    /// Default sample rate
    /// </summary>
    public double DefaultSampleRate;
    /// <summary>
    /// Is the device an output ?
    /// </summary>
    public bool IsInput => this.MaxInputChannels > 0;
    /// <summary>
    /// Is the device an output ?
    /// </summary>
    public bool IsOutput => this.MaxOutputChannels > 0;
}

/// <summary>
/// Structure to match AudioContext in the C library
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AudioContext
{
    /// <summary>
    /// Audio stream pointer
    /// </summary>
    public IntPtr Stream;
    /// <summary>
    /// Current sample rate
    /// </summary>
    public int SampleRate;
    /// <summary>
    /// Number of channels used
    /// </summary>
    public int NumChannels;
    /// <summary>
    /// Is the audio stream running ?
    /// </summary>
    public bool IsRunning;
    /// <summary>
    /// Pointer to the audio buffer
    /// </summary>
    public IntPtr CircularBuffer;
    /// <summary>
    /// Managed callback for audio stream
    /// </summary>
    public IntPtr ManagedCallback; 
}