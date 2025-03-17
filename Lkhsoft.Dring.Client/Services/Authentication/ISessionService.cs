#region

using System.Net.Security;
using System.Security;

#endregion

namespace Lkhsoft.Dring.Client.Services.Authentication;

/// <summary>
/// Service for managing session
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Username of the current user
    /// </summary>
    public string Username { get; internal set; }

    /// <summary>
    /// Is the current user authenticated ?
    /// </summary>
    public bool IsAuthenticated { get; internal set; }

    /// <summary>
    /// Session id of the current user
    /// </summary>
    public string SessionId { get; internal set; }

    /// <summary>
    /// User roles
    /// </summary>
    public IEnumerable<string> Roles { get; internal set; }

    /// <summary>
    /// Login the user
    /// </summary>
    /// <param name="username">Username of the user to be authenticated</param>
    /// <param name="password">Password of the user to be authenticated</param>
    void LogOn(string username, SecureString password);

    /// <summary>
    /// Logout the current user
    /// </summary>
    void LogOff();

    /// <summary>
    /// Gets the SSL stream of the current session
    /// </summary>
    /// <returns>The ssl stream of the established and authenticated connection</returns>
    SslStream? GetSslStream();
}