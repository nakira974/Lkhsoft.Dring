#region

using System.Net.Security;
using System.Security;

#endregion

namespace Lkhsoft.Dring.Client.Services.Authentication;

/// <summary>
/// Session service implementation
/// </summary>
public class SessionService : ISessionService
{
    /// <inheritdoc/>
    public string Username { get; set; }

    /// <inheritdoc/>
    public bool IsAuthenticated { get; set; }

    /// <inheritdoc/>
    public string SessionId { get; set; }

    /// <inheritdoc/>
    public IEnumerable<string> Roles { get; set; }

    /// <summary>
    /// Authentication service
    /// </summary>
    private readonly IAuthService _authService;

    /// <summary>
    /// Default constructor
    /// </summary>
    public SessionService(IAuthService authService)
    {
        _authService = authService;
        Username = string.Empty;
        IsAuthenticated = false;
        SessionId = string.Empty;
        Roles = new List<string>();
    }

    /// <inheritdoc/>
    public async Task LogOn(string username, SecureString password)
    {
        try
        {
            if (!await _authService.LogOn(username, password)) throw new InvalidOperationException("Invalid credentials");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Logon failed: {e.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task LogOff()
    {
        try
        {
            if (!await _authService.LogOff()) throw new InvalidOperationException("Logoff failed");
            ;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Logoff failed: {e.Message}");
        }
    }

    /// <inheritdoc/>
    public SslStream? GetSslStream()
    {
        return _authService.SslStream;
    }
}