
namespace Lkhsoft.Dring.Client.Pages;

public partial class AudioStreamTest : ContentPage
{
    public AudioStreamTest(AudioStreamPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
        pageModel.CanvasView = VideoCanvas;
    }

    private void OnPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        if (BindingContext is AudioStreamPageModel vm)
        {
            vm.DrawFrame(e.Surface, e.Info);
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is AudioStreamPageModel vm)
        {
            vm.StopVideo();
        }
        base.OnDisappearing();
    }
}