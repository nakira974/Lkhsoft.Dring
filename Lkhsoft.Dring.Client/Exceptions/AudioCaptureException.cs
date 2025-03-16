using Device = Lkhsoft.Dring.Client.Services.Device;

namespace Lkhsoft.Dring.Client.Exceptions;

/// <summary>
/// Exception thrown when audio capture fails
/// </summary>
public class AudioCaptureException(Device device, string message) : Exception(message)
{
    /// <summary>
    /// Device that caused the exception
    /// </summary>
    public readonly Device Device = device;
}