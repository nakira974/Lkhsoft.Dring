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
            _sessionService.LogOn(UserName, securePassword);
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        catch (Exception ex)
        {
            AuthErrorMessage = ex.Message;
            IsErrorMessageVisible = true;
        }
    }
}