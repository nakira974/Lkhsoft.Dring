using System.Runtime.InteropServices;
using SkiaSharp;

namespace Lkhsoft.Dring.Client.Services.Multimedia;

/// <summary>
/// Video service interface
/// </summary>
public interface IVideoService : IDisposable
{
     /// <summary>
     /// Native thread callback for video frame
     /// </summary>
     [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
     public delegate void FrameCallback(IntPtr data, int width, int height, int channels, IntPtr userData);
     
     /// <summary>
     /// Gets the frame from the video device
     /// </summary>
     /// <param name="deviceIndex">Host video index</param>
     /// <param name="width">Device width</param>
     /// <param name="height">Device height</param>
     /// <param name="channels">Device channels</param>
     /// <returns>A video stream</returns>
     byte[] CaptureImage(int deviceIndex, out int width, out int height, out int channels);
     
     /// <summary>
     /// Lists the available video devices
     /// </summary>
     /// <returns>Host video device</returns>
     IEnumerable<HostVideoDevice> GetVideoDevices();

     /// <summary>
     /// Starts the video stream
     /// </summary>
     /// <param name="frameHandler">Skia frame handler</param>
     void Start(Action<SKBitmap> frameHandler);

     /// <summary>
     /// Configures the video service
     /// </summary>
     /// <param name="deviceIndex">Selected device index</param>
     /// <param name="targetFps">target FPS</param>
     void Configure(int deviceIndex, int targetFps = 30);
}