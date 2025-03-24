#region

using Lkhsoft.Dring.Client.Services.Multimedia;

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
    public AudioDevice(HostAudioDevice audioHostAudioDevice)
    {
        Name = audioHostAudioDevice.Name;
        IsInput = audioHostAudioDevice.IsInput;
        IsOutput = audioHostAudioDevice.IsOutput;
        HostApiIndex = audioHostAudioDevice.HostApiIndex;
        HostApiDeviceIndex = audioHostAudioDevice.HostApiDeviceIndex;
        HostApiType = audioHostAudioDevice.HostAudioApiType;
        DefaultSampleRate = audioHostAudioDevice.DefaultSampleRate;
        MaxInputChannels = audioHostAudioDevice.MaxInputChannels;
        MaxOutputChannels = audioHostAudioDevice.MaxOutputChannels;
    }

    /// <summary>
    /// Host API index
    /// </summary>
    public int HostApiIndex { get; set; }

    /// <summary>
    /// Index of the device in host API
    /// </summary>
    public int HostApiDeviceIndex { get; set; }

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
    /// Device default sample rate
    /// </summary>
    public double DefaultSampleRate { get; set; }

    /// <summary>
    /// Maximum output channels
    /// </summary>
    public int MaxInputChannels { get; set; }

    /// <summary>
    /// Maximum output channels
    /// </summary>
    public int MaxOutputChannels { get; set; }

    /// <summary>
    /// Audio API type
    /// </summary>
    public HostAudioApiType HostApiType { get; set; }
}