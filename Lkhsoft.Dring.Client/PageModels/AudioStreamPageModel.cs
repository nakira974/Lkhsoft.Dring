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
    private AudioDevice _selectedAudioDevice;
    private bool _isStreaming;
    private string _statusMessage;

    public ObservableCollection<AudioDevice> AudioDevices { get; } = [];

    public AudioDevice SelectedAudioDevice
    {
        get => _selectedAudioDevice;
        set
        {
            _selectedAudioDevice = value;
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
        AudioDevices.Clear();
        LoadAudioDevices();

        // Commandes
        StartStreamingCommand = new Command(StartStreaming);
        StopStreamingCommand = new Command(StopStreaming);
    }

    private void LoadAudioDevices()
    {
        var devices = _audioService.GetAudioDevices();
        foreach (var device in devices.Where(x => x.IsInput)) AudioDevices.Add(new AudioDevice(device));
    }

    private void StartStreaming()
    {
        if (SelectedAudioDevice is null)
        {
            StatusMessage = "Please select an audio device.";
            return;
        }

        // Démarrer la capture audio
        _audioService.StartCapture(SelectedAudioDevice.HostApiIndex, 44100, 2, 1024); // 44.1 kHz, 2 canaux

        IsStreaming = true;
        StatusMessage = "Streaming started.";
    }

    private void StopStreaming()
    {
        // Arrêter la capture audio
        _audioService.StopCapture();

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