namespace Lkhsoft.Dring.Client.Pages;

public partial class AudioStreamTest : ContentPage
{
    public AudioStreamTest(AudioStreamPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}