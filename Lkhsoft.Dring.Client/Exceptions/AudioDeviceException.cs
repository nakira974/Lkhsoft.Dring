#region

using System.Globalization;
#endregion

namespace Lkhsoft.Dring.Client.Exceptions;

/// <summary>
/// Audio device exception
/// </summary>
public class AudioDeviceException : IoDeviceException
{
    /// <inheritdoc />
    public AudioDeviceException(int deviceIndex, string message, Exception ex) 
        : base(Audio, deviceIndex, message, ex)
    {
        
    }
    
    /// <inheritdoc />
    public AudioDeviceException(string message, Exception ex) 
        : base(Audio, message, ex)
    {
        
    }
}