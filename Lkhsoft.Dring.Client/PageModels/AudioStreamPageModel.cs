#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lkhsoft.Dring.Client.Models;

#endregion

namespace Lkhsoft.Dring.Client.PageModels;

public partial class AudioStreamPageModel : INotifyPropertyChanged
{
    private readonly IAudioService _audioService;
    private AudioDevice _selectedInputInputAudioDevice;
    private AudioDevice _selectedOutputAudioDevice;
    private bool _isStreaming;
    private string _statusMessage;

    public ObservableCollection<AudioDevice> InputAudioDevices { get; } = [];
    public ObservableCollection<AudioDevice> OutputAudioDevices { get; } = [];

    public AudioDevice SelectedInputAudioDevice
    {
        get => _selectedInputInputAudioDevice;
        set
        {
            _selectedInputInputAudioDevice = value;
            OnPropertyChanged();
        }
    }

    public AudioDevice SelectedOutputAudioDevice
    {
        get => _selectedOutputAudioDevice;
        set
        {
            _selectedOutputAudioDevice = value;
            OnPropertyChanged();
        }
    }

    public bool IsStreaming
    {
        get => _isStreaming;
        set
        {
            _isStreaming = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotStreaming));
        }
    }

    public bool IsNotStreaming => !_isStreaming;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartStreamingCommand { get; }
    public ICommand StopStreamingCommand { get; }


    public AudioStreamPageModel(IAudioService audioService)
    {
        _audioService = audioService;

        // Charger les périphériques audio
        InputAudioDevices.Clear();
        OutputAudioDevices.Clear();
        LoadAudioDevices();

        // Commandes
        StartStreamingCommand = new Command(StartStreaming);
        StopStreamingCommand = new Command(StopStreaming);
    }

    private void LoadAudioDevices()
    {
        var devices = _audioService.GetAudioDevices();
        foreach (var device in devices.Where(x => x.IsInput)) InputAudioDevices.Add(new AudioDevice(device));
        foreach (var device in devices.Where(x => !x.IsInput)) OutputAudioDevices.Add(new AudioDevice(device));
    }

    private void StartStreaming()
    {
        if (SelectedInputAudioDevice is null)
        {
            StatusMessage = "Please select an audio device.";
            return;
        }

        // Démarrer la capture audio
        _audioService.StartCapture(SelectedInputAudioDevice.HostApiDeviceIndex,
            (int) SelectedInputAudioDevice.DefaultSampleRate, SelectedInputAudioDevice.MaxInputChannels,
            1024); // 44.1 kHz, 2 canaux
        _audioService.StartPlayBack(SelectedOutputAudioDevice.HostApiDeviceIndex,
            (int) SelectedOutputAudioDevice.DefaultSampleRate, SelectedOutputAudioDevice.MaxOutputChannels, 1024);
        IsStreaming = true;
        StatusMessage = "Streaming started.";
    }

    private void StopStreaming()
    {
        // Arrêter la capture audio
        _audioService.StopEngine();

        IsStreaming = false;
        StatusMessage = "Streaming stopped.";
    }

    private void OnAudioDataReceived(float[] data, int size)
    {
        // Envoyer les données audio via TCP
        // (Implémentation à compléter)
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}