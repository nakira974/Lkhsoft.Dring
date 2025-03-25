#region

using Lkhsoft.Dring.Client.Models;
using Lkhsoft.Dring.Client.Services.Multimedia;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

#endregion

namespace Lkhsoft.Dring.Client.PageModels;

public partial class AudioStreamPageModel : INotifyPropertyChanged
{
    private readonly IAudioService _audioService;
    private readonly IVideoService _videoService;
    private CancellationTokenSource _cts;

    private AudioDevice _selectedInputInputAudioDevice;
    private AudioDevice _selectedOutputAudioDevice;
    private HostVideoDevice _selectedInputVideoDevice;
    private bool _isStreaming;
    private string _statusMessage;
    private bool _isVideoRunning;
    private SKBitmap? _currentFrame = new SKBitmap(1, 1);

    public ObservableCollection<AudioDevice> InputAudioDevices { get; } = [];
    public ObservableCollection<AudioDevice> OutputAudioDevices { get; } = [];
    public ObservableCollection<VideoDevice> VideoDevices { get; } = [];


    public bool ShowControls => !IsStreaming;
    public SKCanvasView? CanvasView { get; set; }

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
    
    public HostVideoDevice SelectedVideoDevice
    {
        get => _selectedInputVideoDevice;
        set
        {
            _selectedInputVideoDevice = value;
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
    
    public ICommand StopVideoCommand { get; }
    
    public ICommand StartVideoCommand { get; }


    public AudioStreamPageModel(IAudioService audioService, IVideoService videoService)
    {
        _audioService = audioService;
        _videoService = videoService;

        // Charger les périphériques audio
        InputAudioDevices.Clear();
        OutputAudioDevices.Clear();
        LoadAudioDevices();
        LoadVideoDevices();

        // Commandes
        StartStreamingCommand = new Command(StartStreaming);
        StopStreamingCommand = new Command(StopStreaming);
        StartVideoCommand = new Command(() =>
        {
            IsStreaming = true;
            StartVideo();
        });
        
        StopVideoCommand = new Command(() =>
        {
            IsStreaming = false;
            StopVideo();
        });
    }

    private void LoadAudioDevices()
    {
        var devices = _audioService.GetAudioDevices().ToArray();
        foreach (var device in devices.Where(x => x.IsInput)) InputAudioDevices.Add(new AudioDevice(device));
        foreach (var device in devices.Where(x => !x.IsInput)) OutputAudioDevices.Add(new AudioDevice(device));
    }

    private void LoadVideoDevices()
    {
        var devices = _videoService.GetVideoDevices().ToArray();
        foreach (var device in devices) VideoDevices.Add(new VideoDevice(device));
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
    
    private async void StartVideo()
    {
        // Annuler toute opération précédente
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        try
        {
            await StartVideoReceiver(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Réception vidéo annulée");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur: {ex.Message}");
        }
    }

    public void UpdateFrame(byte[] jpegData, int width, int height, int channels)
    {
        try
        {
            if (jpegData == null || jpegData.Length == 0)
            {
                Debug.WriteLine("Aucune donnée JPEG reçue");
                return;
            }

            // Utilisation de SKCodec pour le décodage optimisé
            using var stream = new MemoryStream(jpegData);
            using var codec = SKCodec.Create(stream);

            // Recyclage du bitmap existant
            if (_currentFrame == null ||
                _currentFrame.Width != codec.Info.Width ||
                _currentFrame.Height != codec.Info.Height)
            {
                _currentFrame?.Dispose();
                _currentFrame = new SKBitmap(codec.Info.Width, codec.Info.Height,
                                           SKColorType.Rgba8888, SKAlphaType.Premul);
            }

            // Décodage direct dans le bitmap existant
            var result = codec.GetPixels(_currentFrame.Info, _currentFrame.GetPixels());
            if (result != SKCodecResult.Success)
            {
                Debug.WriteLine($"Erreur de décodage: {result}");
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    CanvasView?.InvalidateSurface();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur d'invalidation: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur UpdateFrame: {ex.Message}");

            // Fallback: Afficher une image d'erreur
            Device.BeginInvokeOnMainThread(() =>
            {
                _currentFrame?.Dispose();
                _currentFrame = CreateErrorBitmap(width, height);
                CanvasView?.InvalidateSurface();
            });
        }
    }

    private SKBitmap CreateErrorBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(Math.Max(1, width), Math.Max(1, height));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Red);

        using var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 24,
            IsAntialias = true
        };

        canvas.DrawText("Erreur de flux", 10, height / 2, paint);
        return bitmap;
    }

    public void DrawFrame(SKSurface surface, SKImageInfo info)
    {
        if (_currentFrame == null) return;

        var canvas = surface.Canvas;
        canvas.Clear();

        // Calcul du ratio de redimensionnement optimal
        float scale = Math.Min(
            (float)info.Width / _currentFrame.Width,
            (float)info.Height / _currentFrame.Height
        );

        var destRect = new SKRect(
            0, 0,
            _currentFrame.Width * scale,
            _currentFrame.Height * scale
        );

        canvas.DrawBitmap(_currentFrame, destRect);
    }


    public void StopVideo()
    {
        _cts?.Cancel();
        _currentFrame?.Dispose();
        _currentFrame = null;
        CanvasView?.InvalidateSurface();
    }


    private async Task StartVideoReceiver(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var image = _videoService.CaptureVideoStream(SelectedVideoDevice.Index,
                out var width, out var height, out var channels);

            if (image != null && image.Length > 0)
            {
                UpdateFrame(image, width, height, 3);
            }

            // Petit délai pour éviter de saturer le UI thread
            await Task.Delay(30, token); // ~33 FPS
        }
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