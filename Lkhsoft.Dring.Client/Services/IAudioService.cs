namespace Lkhsoft.Dring.Client.Services;

/// <summary>
/// Interface for audio service implementation
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Audio data callback delegate
    /// </summary>
    public delegate void AudioDataCallback(float[] data, int size);

    /// <summary>
    /// Starts capturing audio
    /// </summary>
    /// <param name="hostApiIndex">Index of the device to be used</param>
    /// <param name="sampleRate">Sample rate of the capture</param>
    /// <param name="numChannels">Channels to be used</param>
    /// <param name="bufferCapacity"> Buffer capacity</param>
    void StartCapture(int hostApiIndex, int sampleRate, int numChannels, int bufferCapacity);

    /// <summary>
    /// Stops capturing audio
    /// </summary>
    void StopCapture();

    /// <summary>
    /// Gets the audio data from the audio context
    /// </summary>
    /// <returns></returns>
    float[] GetAudioData(int bufferSize);

    public void RegisterCallback(AudioDataCallback callback);

    /// <summary>
    /// Gets the available audio devices on the system
    /// </summary>
    /// <returns>Available audio devices on the system</returns>
    Device[] GetAudioDevices();
}