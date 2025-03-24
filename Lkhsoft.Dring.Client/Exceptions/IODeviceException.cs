using System.Globalization;

namespace Lkhsoft.Dring.Client.Exceptions;

/// <summary>
/// IO device exception
/// </summary>
public abstract class IoDeviceException : Exception
{

    /// <summary>
    /// Audio device type
    /// </summary>
    protected const string Audio = "audio";
    
    /// <summary>
    /// Video device type
    /// </summary>
    protected const string Video = "video";
    
    /// <summary>
    /// Device type
    /// </summary>
    protected readonly string DeviceType;

    /// <summary>
    /// Faulted device index
    /// </summary>
    protected readonly int DeviceIndex;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="deviceType">Device type</param>
    /// <param name="deviceIndex">Device index</param>
    /// <param name="message">Exception message</param>
    /// <param name="ex">Exception(s) stack</param>
    protected IoDeviceException(string deviceType, int deviceIndex, string message, Exception ex) 
        : base(String.Format(CultureInfo.CurrentCulture, "Exception on {0} device N°{1} : {2}", 
            deviceType, deviceIndex, message), ex)
    {
        this.DeviceType = deviceType;
        this.DeviceIndex = deviceIndex;
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    protected IoDeviceException(string deviceType, string message, Exception ex) 
        : base(message, ex)
    {
        this.DeviceType = deviceType;
        this.DeviceIndex = Int32.MinValue;
    }
}