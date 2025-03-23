using System.Runtime.InteropServices;

namespace Lkhsoft.Dring.Client.Services.Multimedia;

/// <summary>
/// Video service interface
/// </summary>
public interface IVideoService
{
     
     /// <summary>
     /// Gets the frame from the video device
     /// </summary>
     /// <param name="deviceIndex">Host video index</param>
     /// <param name="width">Device width</param>
     /// <param name="height">Device height</param>
     /// <param name="channels">Device channels</param>
     /// <returns>A video stream</returns>
     byte[] CaptureVideoStream(int deviceIndex, out int width, out int height, out int channels);
     
     /// <summary>
     /// Lists the available video devices
     /// </summary>
     /// <returns>Host video device</returns>
     IEnumerable<HostVideoDevice> GetVideoDevices();
}