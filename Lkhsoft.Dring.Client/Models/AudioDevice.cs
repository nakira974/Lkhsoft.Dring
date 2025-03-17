#region

using Device = Lkhsoft.Dring.Client.Services.Device;

#endregion

namespace Lkhsoft.Dring.Client.Models;

/// <summary>
/// Audio device view model
/// </summary>
public class AudioDevice
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public AudioDevice(Device audioDevice)
    {
        Name = audioDevice.Name;
        IsInput = audioDevice.IsInput;
        IsOutput = audioDevice.IsOutput;
        HostApiIndex = audioDevice.HostApiIndex;
        Device = audioDevice;
    }

    public int HostApiIndex { get; set; }

    /// <summary>
    /// Audio device name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Is the device an input ?
    /// </summary>
    public bool IsInput { get; set; }

    /// <summary>
    /// Is the device an output ?
    /// </summary>
    public bool IsOutput { get; set; }

    /// <summary>
    /// Device
    /// </summary>
    public Device Device { get; set; }
}