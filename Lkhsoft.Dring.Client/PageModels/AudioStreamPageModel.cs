using Lkhsoft.Dring.Client.Models;
using Lkhsoft.Dring.Client.Services.Multimedia;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkiaSharp.Views.Maui.Controls;

namespace Lkhsoft.Dring.Client.PageModels;

public partial class AudioStreamPageModel : INotifyPropertyChanged
{
    private readonly IAudioService _audioService;
    private readonly IVideoService _videoService;
    private SKBitmap? _currentFrame = new SKBitmap(1, 1);
    private bool _isStreaming;

    public ObservableCollection<AudioDevice> InputAudioDevices { get; } = [];
    public ObservableCollection<AudioDevice> OutputAudioDevices { get; } = [];
    public ObservableCollection<VideoDevice> VideoDevices { get; } = [];

    public AudioDevice SelectedInputAudioDevice { get; set; }
    public AudioDevice SelectedOutputAudioDevice { get; set; }
    public VideoDevice SelectedVideoDevice { get; set; }
    public SKCanvasView? CanvasView { get; set; }

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

    public ICommand StartStreamingCommand { get; }
    public ICommand StopStreamingCommand { get; }
    public ICommand StartVideoCommand { get; }
    public ICommand StopVideoCommand { get; }

    public AudioStreamPageModel(IAudioService audioService, IVideoService videoService)
    {
        _audioService = audioService;
        _videoService = videoService;

        LoadDevices();

        StartStreamingCommand = new Command(StartStreaming);
        StopStreamingCommand = new Command(StopStreaming);
        StartVideoCommand = new Command(StartVideo);
        StopVideoCommand = new Command(StopVideo);
    }

    private void LoadDevices()
    {
        InputAudioDevices.Clear();
        OutputAudioDevices.Clear();
        VideoDevices.Clear();

        foreach (var device in _audioService.GetAudioDevices().Where(x => x.IsInput))
            InputAudioDevices.Add(new AudioDevice(device));

        foreach (var device in _audioService.GetAudioDevices().Where(x => !x.IsInput))
            OutputAudioDevices.Add(new AudioDevice(device));

        foreach (var device in _videoService.GetVideoDevices())
            VideoDevices.Add(new VideoDevice(device));
    }

    private void UpdateVideoFrame(SKBitmap frame)
    {
        _currentFrame?.Dispose();
        _currentFrame = frame;
        try
        {
            CanvasView?.InvalidateSurface();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'invalidation: {ex.Message}");
        }
    }

    public SKBitmap VideoFrame 
    {
        get => _currentFrame;
        private set
        {
            _currentFrame?.Dispose();
            _currentFrame = value;
            OnPropertyChanged();
        
            // Force le redraw explicitement
            CanvasView?.InvalidateSurface();
        }
    }


    private void StartStreaming()
    {
        if (SelectedInputAudioDevice == null || SelectedOutputAudioDevice == null)
            return;

        _audioService.StartCapture(SelectedInputAudioDevice.HostApiDeviceIndex,
            (int)SelectedInputAudioDevice.DefaultSampleRate, 
            SelectedInputAudioDevice.MaxInputChannels, 
            1024);

        _audioService.StartPlayBack(SelectedOutputAudioDevice.HostApiDeviceIndex,
            (int)SelectedOutputAudioDevice.DefaultSampleRate,
            SelectedOutputAudioDevice.MaxOutputChannels,
            1024);

        IsStreaming = true;
    }

    private void StartVideo()
    {
        if (SelectedVideoDevice is null)
            return;
        
        _videoService.Configure(SelectedVideoDevice.Index);
        
        _videoService.Start(frame => 
            MainThread.BeginInvokeOnMainThread(() => UpdateVideoFrame(frame)));
        
        IsStreaming = true;
    }

    internal void StopStreaming()
    {
        _audioService.StopEngine();
        IsStreaming = false;
    }

    internal void StopVideo()
    {
        _videoService.Dispose();
        IsStreaming = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}