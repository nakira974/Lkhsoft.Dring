
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Lkhsoft.Dring.Client.Pages;

public partial class AudioStreamTest : ContentPage
{  
    private readonly AudioStreamPageModel _viewModel;
    public AudioStreamTest(AudioStreamPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = pageModel;
        pageModel.CanvasView = VideoCanvas;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;
        canvas.Clear();

        if (_viewModel.VideoFrame is null) return;

        // Calculate aspect ratio
        float scale = Math.Min(
            (float)e.Info.Width / _viewModel.VideoFrame.Width,
            (float)e.Info.Height / _viewModel.VideoFrame.Height
        );

        var destRect = new SKRect(
            (e.Info.Width - _viewModel.VideoFrame.Width * scale) / 2,
            (e.Info.Height - _viewModel.VideoFrame.Height * scale) / 2,
            (e.Info.Width + _viewModel.VideoFrame.Width * scale) / 2,
            (e.Info.Height + _viewModel.VideoFrame.Height * scale) / 2
        );

        canvas.DrawBitmap(_viewModel.VideoFrame, destRect);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopVideo();
        _viewModel.StopStreaming();
    }
}