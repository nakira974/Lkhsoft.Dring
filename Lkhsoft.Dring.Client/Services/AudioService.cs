#region

using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace Lkhsoft.Dring.Client.Services;

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
    /// TCP client for audio streaming
    /// </summary>
    private TcpClient _tcpClient;

    /// <summary>
    /// Network stream for audio streaming
    /// </summary>
    private SslStream _sslStream;

    /// <summary>
    /// Native library name
    /// </summary>
    private const string nativeLibraryName = "libaudio_stream";

    /// <summary>
    /// Default server port
    /// </summary>
    private const ushort defaultServerPort = 9091;

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
        if (!Audio_Initialize()) throw new Exception("Failed to initialize audio library");
        RegisterCallback(OnAudioDataReceived);
    }

    /// <inheritdoc/>
    public void StartCapture(int hostApiIndex, int sampleRate, int numChannels, int bufferCapacity)
    {
        if (!Audio_StartCapture(_audioContext, hostApiIndex, sampleRate, numChannels, bufferCapacity))
            throw new Exception("Failed to start audio capture.");
    }

    /// <inheritdoc/>
    public void RegisterCallback(IAudioService.AudioDataCallback callback)
    {
        _audioDataCallback = callback; // Garder une référence pour éviter le GC
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        RegisterAudioDataCallback(_audioContext, callbackPtr);
    }

    /// <inheritdoc/>
    public float[] GetAudioData(int bufferSize)
    {
        var buffer = new float[bufferSize];
        var samplesRead = Audio_GetAudioData(_audioContext, buffer, bufferSize);
        if (samplesRead == 0) throw new Exception("No audio data available.");
        return buffer;
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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="size"></param>
    private void OnAudioDataReceived(float[] data, int size)
    {
        if (_tcpClient is null || !_tcpClient.Connected)
            try
            {
                // Se connecter au serveur TCP
                _tcpClient = new TcpClient("127.0.0.1", 9091);

                // Créer un SslStream à partir du NetworkStream
                _sslStream = new SslStream(_tcpClient.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate));

                _sslStream.AuthenticateAsClient("localhost");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de connexion SSL : " + ex.Message);
                return;
            }

        // Convertir les données audio en bytes
        var buffer = new byte[size * sizeof(float)];
        Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);

        try
        {
            // Envoyer les données via SSL
            _sslStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur d'envoi de données SSL : " + ex.Message);
        }
    }

    /// <summary>
    /// SSL server certificate validation
    /// </summary>
    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // En mode DEBUG, accepter les certificats auto-signés
#if DEBUG
        if (sslPolicyErrors == SslPolicyErrors.None ||
            sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            return true;
#else
    // En mode RELEASE, appliquer une validation stricte
    if (sslPolicyErrors == SslPolicyErrors.None)
    {
        return true;
    }
#endif

        Console.WriteLine("Erreur de certificat SSL : " + sslPolicyErrors);
        return false;
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
    public bool IsInput => MaxInputChannels > 0;

    /// <summary>
    /// Is the device an output ?
    /// </summary>
    public bool IsOutput => MaxOutputChannels > 0;
}

/// <summary>
/// Structure to match AudioContext in the C library
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AudioContext
{
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
    public CircularBuffer CircularBuffer;

    /// <summary>
    /// Managed callback for audio stream (pointeur de fonction en C)
    /// </summary>
    public IntPtr ManagedCallback;
}

/// <summary>
/// Structure to match CircularBuffer in the C library
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CircularBuffer
{
    /// <summary>
    /// Audio data buffer (float* en C)
    /// </summary>
    public IntPtr Buffer;

    /// <summary>
    /// Buffer capacity
    /// </summary>
    public int Capacity;

    /// <summary>
    /// Buffer's head
    /// </summary>
    public int Head;

    /// <summary>
    /// Buffer's tail
    /// </summary>
    public int Tail;

    /// <summary>
    /// Buffer size
    /// </summary>
    public int Size;
}

#endregion