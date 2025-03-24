namespace Lkhsoft.Dring.Client.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}