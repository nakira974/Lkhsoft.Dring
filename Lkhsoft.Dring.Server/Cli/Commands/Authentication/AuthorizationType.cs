namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
/// System authorization type
/// </summary>
public enum AuthorizationType
{
    /// <summary>
    /// No authorization required
    /// </summary>
    Guest,

    /// <summary>
    /// User authorization required
    /// </summary>
    User,

    /// <summary>
    /// Admin authorization required
    /// </summary>
    Admin
}