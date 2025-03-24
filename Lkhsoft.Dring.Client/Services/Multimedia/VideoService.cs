using System.Reflection;
using System.Runtime.InteropServices;
using Lkhsoft.Dring.Client.Exceptions;

namespace Lkhsoft.Dring.Client.Services.Multimedia;

/// <summary>
/// Video service implementation
/// </summary>
internal class VideoService : IVideoService
{

    /// <inheritdoc />
    public byte[] CaptureVideoStream(int deviceIndex, out int width, out int height, out int channels)
    {
        IntPtr framePtr;
        try
        {
            framePtr = CaptureFrame(deviceIndex, out width, out height, out channels);
        }
        catch(Exception ex)
        {
            throw new VideoDeviceException("Failed to capture video stream", ex);
        }
        
        if (framePtr == IntPtr.Zero) throw new Exception("Failed to capture video stream");
        var frame = new byte[width * height * channels];
        Marshal.Copy(framePtr, frame, 0, frame.Length);
        
        // free du pointeur alloué par la fonction native
        try
        {
            FreeFrame(framePtr);
        }
        catch(Exception ex)
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
        catch(Exception ex)
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

    #region NATIVE METHODS
    /// <summary>
    /// Captures a frame from the video device
    /// </summary>
    /// <param name="deviceIndex">Device index</param>
    /// <param name="width">Device width</param>
    /// <param name="height">Device height</param>
    /// <param name="channels">Device channels</param>
    /// <returns>A video stream from the device</returns>
    [DllImport(NativeLibraries.MultimediaStream, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr CaptureFrame(int deviceIndex, out int width, out int height, out int channels);
    
    /// <summary>
    /// Lists the available video devices
    /// </summary>
    /// <param name="count">Devices count</param>
    /// <returns>Host video device</returns>
    [DllImport(NativeLibraries.MultimediaStream, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVideoDevices(out int count);

    /// <summary>
    /// Frees the frame
    /// </summary>
    /// <param name="frame">Frame stream to be free</param>
    [DllImport(NativeLibraries.MultimediaStream, CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeFrame(IntPtr frame);

    /// <summary>
    /// Frees the video devices array
    /// </summary>
    /// <param name="devices">Video devices struct array to be free</param>
    [DllImport(NativeLibraries.MultimediaStream, CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeVideoDevices(IntPtr devices);
    #endregion
}

#region NATIVE STRUCTS
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