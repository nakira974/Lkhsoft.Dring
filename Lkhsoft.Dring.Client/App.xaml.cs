#region

using Lkhsoft.Dring.Client.Services.Authentication;

#endregion

namespace Lkhsoft.Dring.Client;

public partial class App : Application
{
    private readonly ISessionService _sessionService;
    private readonly LoginPageModel _loginPageModel;

    public App(ISessionService sessionService, LoginPageModel loginPageModel)
    {
        _sessionService = sessionService;
        _loginPageModel = loginPageModel;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return !_sessionService.IsAuthenticated
            ? new Window(new LoginPage(_loginPageModel))
            : new Window(new AppShell());
    }
}