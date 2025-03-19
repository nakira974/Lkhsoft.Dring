#region

using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lkhsoft.Dring.Client.Services.Authentication;

#endregion

namespace Lkhsoft.Dring.Client.PageModels;

/// <summary>
/// Login page model
/// </summary>
public partial class LoginPageModel : ObservableObject
{
    private readonly ISessionService _sessionService;

    [ObservableProperty] private string _userName = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _authErrorMessage = string.Empty;

    [ObservableProperty] private bool _isErrorMessageVisible = false;

    public LoginPageModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task Login()
    {
        // Convertir le mot de passe en SecureString
        using var securePassword = new SecureString();
        foreach (var c in Password) securePassword.AppendChar(c);

        // Appeler la méthode d'authentification avec SecureString
        try
        {
            await _sessionService.LogOn(UserName, securePassword);

            var currentApp = Application.Current;


            // Fermer la Window actuelle (celle de la page de login)
            if (currentApp is not null && currentApp.Windows.Count > 0)
            {
                var currentWindow = currentApp.Windows[0];
                currentWindow.Destroying += OnAppQuitting;
                currentWindow.Page = new AppShell();
            }
        }
        catch (Exception ex)
        {
            AuthErrorMessage = ex.Message;
            IsErrorMessageVisible = true;
        }
    }
    
    /// <summary>
    /// On app quitting handle
    /// </summary>
    private async void OnAppQuitting(object? sender, EventArgs e)
    {
        try
        {
            // Appeler le service de shutdown
            await _sessionService.LogOff();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Logoff failed: {ex.Message}");
        }
    }
}