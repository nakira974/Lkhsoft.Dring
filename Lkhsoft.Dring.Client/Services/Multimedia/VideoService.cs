using System.Runtime.InteropServices;
using Lkhsoft.Dring.Client.Exceptions;
using SkiaSharp;

namespace Lkhsoft.Dring.Client.Services.Multimedia;

/// <summary>
/// Video service implementation
/// </summary>
internal partial class VideoService : IVideoService
{
    /// <summary>
    /// Native thread handle
    /// </summary>
    private IntPtr _handle;

    /// <summary>
    /// Native thread frame callback
    /// </summary>
    private IVideoService.FrameCallback _frameCallback;

    /// <summary>
    /// SkiaSharp bitmap frame handler
    /// </summary>
    private Action<SKBitmap> _frameHandler;

    /// <summary>
    /// Bitmap lock object
    /// </summary>
    private readonly object _bitmapLock = new();

    public VideoService()
    {
        _frameCallback = OnFrameReceived;
    }

    /// <inheritdoc />
    public void Configure(int deviceIndex, int targetFps = 30)
    {
        var config = new VideoStreamConfig
        {
            DeviceIndex = deviceIndex,
            TargetFps = targetFps,
            UserData = IntPtr.Zero
        };
        _handle = VideoStreamCreate(ref config);
    }

    /// <inheritdoc />
    public void Start(Action<SKBitmap> frameHandler)
    {
        _frameHandler = frameHandler ?? throw new ArgumentNullException(nameof(frameHandler));
        VideoStreamStart(_handle, _frameCallback);
    }


    /// <inheritdoc />
    public byte[] CaptureImage(int deviceIndex, out int width, out int height, out int channels)
    {
        IntPtr framePtr;
        int bufferSize;
        try
        {
            framePtr = CaptureFrame(deviceIndex, out width, out height, out channels, out bufferSize);
        }
        catch (Exception ex)
        {
            throw new VideoDeviceException("Failed to capture video stream", ex);
        }

        if (framePtr == IntPtr.Zero) throw new Exception("Failed to capture video stream");
        var frame = new byte[bufferSize];
        Marshal.Copy(framePtr, frame, 0, frame.Length);

        // free du pointeur alloué par la fonction native
        try
        {
            FreeFrame(framePtr);
        }
        catch (Exception ex)
        {
            throw new VideoDeviceException("Failed to free video stream", ex);
        }

        return frame;
    }

    /// <inheritdoc />
    public IEnumerable<HostVideoDevice> GetVideoDevices()
    {
        IntPtr devicesPtr;
        int deviceCount;
        try
        {
            devicesPtr = GetVideoDevices(out deviceCount);
        }
        catch (Exception ex)
        {
            throw new VideoDeviceException("Failed to list video devices", ex);
        }

        if (devicesPtr == IntPtr.Zero || deviceCount == 0) throw new Exception("Failed to list video devices");
        var devices = new HostVideoDevice[deviceCount];

        for (var i = 0; i < deviceCount; i++)
            // memcpy(&devices[i], devicesPtr + i * sizeof(Device), sizeof(Device));
            devices[i] = Marshal.PtrToStructure<HostVideoDevice>(devicesPtr + i * Marshal.SizeOf<HostVideoDevice>());

        // free du pointeur alloué par la fonction native
        try
        {
            FreeVideoDevices(devicesPtr);
        }
        catch (Exception ex)
        {
            throw new VideoDeviceException("Could not free video devices", ex);
        }

        return devices;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        VideoStreamFree(_handle);
        _handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    #region PRIVATE METHODS

    /// <summary>
    /// Called when a frame is received from openCV
    /// </summary>
    /// <param name="data">OpenCV frame</param>
    /// <param name="width">OpenCV frame's width</param>
    /// <param name="height">OpenCV frame height</param>
    /// <param name="channels">OpenCV frame's channels</param>
    /// <param name="userData">Native thread user data</param>
    private void OnFrameReceived(IntPtr data, int width, int height, int channels, IntPtr userData)
    {
        try
        {
            // 1. Validation des paramètres
            if (data == IntPtr.Zero || width <= 0 || height <= 0 || channels != 3)
            {
                Console.WriteLine("Paramètres de frame invalides");
                return;
            }

            // 2. Calcul de la taille du buffer
            var bufferSize = width * height * channels;

            // 3. Copie des données vers un buffer managé
            var managedBuffer = new byte[bufferSize];
            Marshal.Copy(data, managedBuffer, 0, bufferSize);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                lock (_bitmapLock)
                {
                    try
                    {
                        // 4. Création d'un bitmap SkiaSharp avec le bon format
                        var info = new SKImageInfo(width, height, SKColorType.Rgba8888);
                        var bitmap = new SKBitmap(info);

                        // 5. Conversion BGR vers RGBA (OpenCV utilise BGR par défaut)
                        unsafe
                        {
                            fixed (byte* srcPtr = managedBuffer)
                            {
                                var dstPtr = (byte*) bitmap.GetPixels();

                                // Parcours de chaque pixel en hauteur
                                for (var y = 0; y < height; y++)
                                {
                                    // Parcours de chaque pixel en largeur
                                    for (var x = 0; x < width; x++)
                                    {
                                        var srcIndex = (y * width + x) * 3;
                                        var dstIndex = (y * width + x) * 4;

                                        // Conversion BGR to RGBA
                                        dstPtr[dstIndex] = srcPtr[srcIndex + 2]; // R
                                        dstPtr[dstIndex + 1] = srcPtr[srcIndex + 1]; // G
                                        dstPtr[dstIndex + 2] = srcPtr[srcIndex]; // B
                                        dstPtr[dstIndex + 3] = 255; // A
                                    }
                                }
                            }
                        }

                        // 6. Notification du handler avec une COPIE du bitmap
                        _frameHandler?.Invoke(bitmap.Copy());

                        // 7. Nettoyage
                        bitmap.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur de traitement d'image: {ex.Message}");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur OnFrameReceived: {ex.Message}");
        }
    }

    #endregion

    #region NATIVE METHODS

    /// <summary>
    /// Captures a frame from the video device
    /// </summary>
    /// <param name="deviceIndex">Device index</param>
    /// <param name="width">Device width</param>
    /// <param name="height">Device height</param>
    /// <param name="channels">Device channels</param>
    /// <returns>A video stream from the device</returns>
    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr CaptureFrame(int deviceIndex, out int width, out int height, out int channels,
        out int bufferSize);

    /// <summary>
    /// Lists the available video devices
    /// </summary>
    /// <param name="count">Devices count</param>
    /// <returns>Host video device</returns>
    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr GetVideoDevices(out int count);

    /// <summary>
    /// Frees the frame
    /// </summary>
    /// <param name="frame">Frame stream to be free</param>
    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void FreeFrame(IntPtr frame);

    /// <summary>
    /// Frees the video devices array
    /// </summary>
    /// <param name="devices">Video devices struct array to be free</param>
    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void FreeVideoDevices(IntPtr devices);

    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr VideoStreamCreate(ref VideoStreamConfig config);

    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void VideoStreamStart(IntPtr handle, IVideoService.FrameCallback callback);

    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void VideoStreamStop(IntPtr handle);

    [LibraryImport(NativeLibraries.MultimediaStream)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void VideoStreamFree(IntPtr handle);

    #endregion
}

#region NATIVE STRUCTS

/// <summary>
/// Video stream configuration
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct VideoStreamConfig
{
    /// <summary>
    /// Device index
    /// </summary>
    public int DeviceIndex;

    /// <summary>
    /// Target FPS
    /// </summary>
    public int TargetFps;

    /// <summary>
    /// User data pointer
    /// </summary>
    public IntPtr UserData;
}

/// <summary>
/// Video device structure
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct HostVideoDevice
{
    /// <summary>
    /// Device index
    /// </summary>
    public int Index;

    /// <summary>
    /// Device name
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Name;
}

#endregion