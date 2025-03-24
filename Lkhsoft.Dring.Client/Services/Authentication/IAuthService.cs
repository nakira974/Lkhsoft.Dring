#region

using System.Net.Security;
using System.Security;

#endregion

namespace Lkhsoft.Dring.Client.Services.Authentication;

/// <summary>
/// Authentication service
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Ssl stream of the established connection
    /// </summary>
    public SslStream? SslStream { get; internal set; }

    /// <summary>
    /// Authenticate the user
    /// </summary>
    /// <param name="username">Username of the user to be authenticated</param>
    /// <param name="password">Password of the user to be authenticated</param>
    /// <returns>True of the user has been authenticated, otherwise false</returns>
    public Task<bool> LogOn(string username, SecureString password);

    /// <summary>
    /// Unauthenticated the user
    /// </summary>
    /// <returns>True if the logoff succeed, otherwise false</returns>
    public Task<bool> LogOff();
}