#region

using Lkhsoft.Dring.Shared.Cli;

#endregion

namespace Lkhsoft.Dring.Shared.Core.Authentication;

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

    /// <summary>
    ///     Registers a new CLI session
    /// </summary>
    /// <param name="session">Session to be registered</param>
    public void RegisterCliSession(CommandParser session);

    /// <summary>
    ///    Returns the current CLI session
    /// </summary>
    /// <returns>The current CLI session</returns>
    public CommandParser GetCliSession();

    /// <summary>
    ///     Current text writer
    /// </summary>
    /// <param name="textWriter">Text writer to be registered</param>
    public void RegisterTextWriter(TextWriter textWriter);

    /// <summary>
    ///     Returns the current text writer
    /// </summary>
    /// <returns></returns>
    public TextWriter GetTextWriter();
}