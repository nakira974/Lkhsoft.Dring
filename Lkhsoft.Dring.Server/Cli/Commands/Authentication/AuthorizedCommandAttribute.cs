namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
/// Attribute to define the authorization of a command
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class AuthorizedCommandAttribute : Attribute
{
    /// <summary>
    /// Allowed authorizations of a command
    /// </summary>
    public ISet<AuthorizationType> Authorizations { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="authorizations">Authorizations of a command</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AuthorizedCommandAttribute(params AuthorizationType[] authorizations)
    {
        Authorizations =
            new HashSet<AuthorizationType>(authorizations ?? throw new ArgumentNullException(nameof(authorizations)));
    }
}