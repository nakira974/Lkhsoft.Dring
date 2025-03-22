#region

using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using Lkhsoft.Dring.Client.Services.Authentication;

#endregion

namespace Lkhsoft.Dring.Client.Services;

/// <summary>
/// Audio service implementation
/// </summary>
public class AudioService : IAudioService
{
    private readonly ISessionService _sessionService;

    /// <summary>
    /// Audio context from native library
    /// </summary>
    private IntPtr _inputAudioContext;

    /// <summary>
    /// Audio context from native library
    /// </summary>
    private IntPtr _outputAudioContext;

    /// <summary>
    /// Audio data callback
    /// </summary>
    private IAudioService.AudioDataCallback _audioDataCallback;

    /// <summary>
    /// Network stream for audio streaming
    /// </summary>
    private SslStream? _sslStream;

    /// <summary>
    /// Native library name
    /// </summary>
    private const string nativeLibraryName = "libmultimedia_stream";

    /// <summary>
    /// Selected audio device
    /// </summary>
    public Device SelectedDevice { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <exception cref="Exception">The native library failed to initialize</exception>
    /// <exception cref="DllNotFoundException">Could not find audio_stream library the assembly</exception>
    public AudioService(ISessionService sessionService)
    {
        _sessionService = sessionService;
        _sslStream = sessionService.GetSslStream();
        LoadNativeLibrary();
        // malloc the audio context
        _inputAudioContext = Marshal.AllocHGlobal(Marshal.SizeOf<AudioContext>());
        _outputAudioContext = Marshal.AllocHGlobal(Marshal.SizeOf<AudioContext>());
        if (!Audio_Initialize()) throw new Exception("Failed to initialize audio library");
        RegisterCallback(OnAudioDataEmitted);
    }

    /// <inheritdoc/>
    public void StartCapture(int hostApiDeviceIndex, int sampleRate, int numChannels, int bufferCapacity)
    {
        if (!Audio_StartCapture(_inputAudioContext, hostApiDeviceIndex, sampleRate, numChannels, bufferCapacity))
            throw new Exception("Failed to start audio capture.");
    }

    /// <inheritdoc/>
    public void StartPlayBack(int hostApiDeviceIndex, int sampleRate, int numChannels, int bufferCapacity)
    {
        if (!Audio_StartPlay(_outputAudioContext, hostApiDeviceIndex, sampleRate, numChannels, bufferCapacity))
            throw new Exception("Failed to start audio playback.");
    }

    /// <inheritdoc/>
    public void RegisterCallback(IAudioService.AudioDataCallback callback)
    {
        _audioDataCallback = callback; // Garder une référence pour éviter le GC
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        RegisterAudioDataCallback(_inputAudioContext, callbackPtr);
    }

    /// <inheritdoc/>
    public float[] GetAudioData(int bufferSize)
    {
        var buffer = new float[bufferSize];
        var samplesRead = Audio_GetAudioData(_inputAudioContext, buffer, bufferSize);
        if (samplesRead == 0) throw new Exception("No audio data available.");
        return buffer;
    }

    /// <inheritdoc/>
    public void SendDataToAudioEngine(float[] audioData)
    {
        var sizeInBytes = audioData.Length * sizeof(float);
        var unmanagedData = Marshal.AllocHGlobal(sizeInBytes);

        Marshal.Copy(audioData, 0, unmanagedData, audioData.Length);

        Audio_AddData(_outputAudioContext, unmanagedData, audioData.Length);

        Marshal.FreeHGlobal(unmanagedData);
    }

    /// <inheritdoc/>
    public void StopEngine()
    {
        if (_inputAudioContext != IntPtr.Zero)
            Audio_StopCapture(_inputAudioContext);
        if (_outputAudioContext != IntPtr.Zero)
            Audio_StopCapture(_outputAudioContext);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Audio_Shutdown();
        Marshal.FreeHGlobal(_inputAudioContext);
        Marshal.FreeHGlobal(_outputAudioContext);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public Device[] GetAudioDevices()
    {
        var devicesPtr = GetAudioDevices(out var deviceCount);

        if (devicesPtr == IntPtr.Zero || deviceCount == 0) throw new Exception("Failed to list audio devices");

        // Convertir le tableau natif en tableau C#
        var devices = new Device[deviceCount];
        for (var i = 0; i < deviceCount; i++)
            // memcpy(&devices[i], devicesPtr + i * sizeof(Device), sizeof(Device));
            devices[i] = Marshal.PtrToStructure<Device>(devicesPtr + i * Marshal.SizeOf<Device>());

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
            NativeLibrary.Load(nativeLibraryName, Assembly.GetCallingAssembly(), DllImportSearchPath.AssemblyDirectory);
        }
        catch (Exception ex)
        {
            throw new DllNotFoundException($"Failed to load native library '{nativeLibraryName}'", ex);
        }
    }

    /// <summary>
    /// Sends recorded audio data to the network
    /// </summary>
    private void OnAudioDataEmitted(IntPtr data, int size)
    {
        // Convertir les données audio en bytes
        var managedData = new float[size];
        Marshal.Copy(data, managedData, 0, size);
        var buffer = new byte[size * sizeof(float)];
        Buffer.BlockCopy(managedData, 0, buffer, 0, buffer.Length);

        try
        {
            // Envoyer les données via SSL
            _sslStream?.Write(buffer, 0, buffer.Length);
            SendDataToAudioEngine(managedData);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur d'envoi de données SSL : " + ex.Message);
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
    private static extern bool Audio_StartCapture(IntPtr context, int hostApiContext, int sampleRate, int numChannels,
        int bufferCapacity);

    /// <summary>
    /// Starts to play back the audio
    /// </summary>
    /// <param name="context">PortAudio context</param>
    /// <param name="hostApiContext">Device index</param>
    /// <param name="sampleRate">Capture sample rate</param>
    /// <param name="numChannels">Number of channels</param>
    /// <param name="bufferCapacity">Buffer capacity</param>
    /// <returns>True if the capture has been correctly initialized, otherwise false</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Audio_StartPlay(IntPtr context, int hostApiContext, int sampleRate, int numChannels,
        int bufferCapacity);

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
    /// <param name="buffer">Buffer to store the audio data</param>
    /// <param name="bufferSize">Buffer size</param>
    /// <returns>A pointer to the audio data</returns>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Audio_GetAudioData(IntPtr context, float[] buffer, int bufferSize);

    /// <summary>
    /// Sets the audio data from the audio stream
    /// </summary>
    /// <param name="context">Audio context</param>
    /// <param name="buffer">Buffer to write the audio data</param>
    /// <param name="bufferSize">Buffer size</param>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Audio_AddData(IntPtr context, IntPtr buffer, int bufferSize);

    /// <summary>
    /// Registers the audio data callback
    /// </summary>
    /// <param name="context"></param>
    /// <param name="callback"></param>
    [DllImport(nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void RegisterAudioDataCallback(IntPtr context, IntPtr callback);

    #endregion
}

#region NATIVE STRUCTURES

/// <summary>
/// Audio device structure
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public record struct Device
{
    /// <summary>
    /// Index of the API
    /// </summary>
    public int HostApiIndex;

    /// <summary>
    /// Index of the device in the API
    /// </summary>
    public int HostApiDeviceIndex;

    /// <summary>
    /// Host API type
    /// </summary>
    public AudioApiType HostApiType;

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
    public bool IsInput => MaxInputChannels > 0;

    /// <summary>
    /// Is the device an output ?
    /// </summary>
    public bool IsOutput => MaxOutputChannels > 0;
}

/// <summary>
/// Audio API type
/// </summary>
public enum AudioApiType
{
    /// <summary>
    /// Audio API in development used while developing support for a new host API 
    /// </summary>
    InDevelopment = 0,

    /// <summary>
    /// DirectSound audio API
    /// </summary>
    DirectSound = 1,

    /// <summary>
    /// MME audio API
    /// </summary>
    Mme = 2,

    /// <summary>
    /// ASIO audio API
    /// </summary>
    Asio = 3,

    /// <summary>
    /// SoundManager audio API
    /// </summary>
    SoundManager = 4,

    /// <summary>
    /// CoreAudio audio API
    /// </summary>
    CoreAudio = 5,

    /// <summary>
    /// OSS audio API
    /// </summary>
    Oss = 7,

    /// <summary>
    /// ALSA audio API
    /// </summary>
    Alsa = 8,

    /// <summary>
    /// AL audio API
    /// </summary>
    Al = 9,

    /// <summary>
    /// BeOS audio API
    /// </summary>
    BeOs = 10,

    /// <summary>
    /// WDM/KS audio API
    /// </summary>
    Wdmks = 11,

    /// <summary>
    /// JACK audio API
    /// </summary>
    Jack = 12,

    /// <summary>
    /// WASAPI audio API
    /// </summary>
    Wasapi = 13,

    /// <summary>
    /// AudioScience HPI audio API
    /// </summary>
    AudioScienceHpi = 14,

    /// <summary>
    /// Steinberg ASIO audio API
    /// </summary>
    AudioIo = 15,

    /// <summary>
    /// PulseAudio audio API
    /// </summary>
    PulseAudio = 16,

    /// <summary>
    /// Sndio audio API
    /// </summary>
    Sndio = 17
}

/// <summary>
/// Structure to match AudioContext in the C library
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AudioContext
{
    /// <summary>
    /// Left phase of the signal
    /// </summary>
    public float LeftPhase;

    /// <summary>
    /// Right phase of the signal
    /// </summary>
    public float RightPhase;

    /// <summary>
    /// Audio stream pointer (PaStream* in C)
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
    /// Is the audio stream running?
    /// </summary>
    [MarshalAs(UnmanagedType.I1)] public bool IsRunning;

    /// <summary>
    /// Audio circular buffer (structure en C, pas un pointeur)
    /// </summary>
    public IntPtr CircularBuffer;

    /// <summary>
    /// Managed callback for audio stream (pointeur de fonction en C)
    /// </summary>
    public IntPtr ManagedCallback;
}

#endregion