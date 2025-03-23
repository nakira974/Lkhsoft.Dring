namespace Lkhsoft.Dring.Client.Exceptions;

/// <summary>
/// Video device exception
/// </summary>
public class VideoDeviceException : IoDeviceException
{
    /// <inheritdoc />
    public VideoDeviceException(int deviceIndex, string message, Exception ex) 
        : base(Video, deviceIndex, message, ex)
    {
        
    }
    
    /// <inheritdoc />
    public VideoDeviceException(string message, Exception ex) 
        : base(Video, message, ex)
    {
        
    }
}