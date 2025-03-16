using Device = Lkhsoft.Dring.Client.Services.Device;

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
        this.Name = audioDevice.Name;
        this.IsInput = audioDevice.IsInput;
        this.IsOutput = audioDevice.IsOutput;
        this.Device = audioDevice;
    }
    
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