namespace Lkhsoft.Dring.Client.Services;

/// <summary>
/// Interface for audio service implementation
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Starts capturing audio
    /// </summary>
    /// <param name="sampleRate">Sample rate of the capture</param>
    /// <param name="numChannels">Channels to be used</param>
    void StartCapture(int sampleRate, int numChannels);

    /// <summary>
    /// Stops capturing audio
    /// </summary>
    void StopCapture();

    /// <summary>
    /// Gets the available audio devices on the system
    /// </summary>
    /// <returns>Available audio devices on the system</returns>
    Device[] GetAudioDevices();

}