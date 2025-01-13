namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
/// Define the session service
/// </summary>
public interface ISessionService
{
    /// <summary>
    ///     Add a session to the service
    /// </summary>
    /// <param name="session">Session to be added</param>
    public void AddSession(Session session);

    /// <summary>
    ///     Remove a session from the service
    /// </summary>
    /// <param name="session">Session to be removed</param>
    public void RemoveSession(Session session);
    
    /// <summary>
    /// Clears all session from the service
    /// </summary>
    public void ClearAllSessions();

    /// <summary>
    ///     Gets a session by its username
    /// </summary>
    public Session? GetSessionByUsername(string username);
}