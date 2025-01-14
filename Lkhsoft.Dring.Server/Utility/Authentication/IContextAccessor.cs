namespace Lkhsoft.Dring.Server.Utility.Authentication;

/// <summary>
///     Session context accessor
/// </summary>
public interface IContextAccessor
{
    /// <summary>
    ///     Returns the current session
    /// </summary>
    public Session GetSession();

    /// <summary>
    ///     Registers a new session
    /// </summary>
    /// <param name="new">New session to register</param>
    public void Register(Session @new);

    /// <summary>
    ///     Unregisters the current session
    /// </summary>
    public void Unregister();

    /// <summary>
    ///     Returns the current role
    /// </summary>
    public Task<string?> GetRole();
}