namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
///     Server session model
/// </summary>
public class Session
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="username">Username authenticated</param>
    public Session(string username)
    {
        Username = username;
    }

    /// <summary>
    ///     Username of the user
    /// </summary>
    public string Username { get; }

    /// <summary>
    ///     Session start time
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    ///     Session end time
    /// </summary>
    public DateTime EndTime { get; private set; }

    /// <summary>
    ///     Start the session
    /// </summary>
    public void Start()
    {
        StartTime = DateTime.Now;
        Console.WriteLine($"Session started for user {Username} at {StartTime}");
    }

    /// <summary>
    ///     End the session
    /// </summary>
    public void End()
    {
        EndTime = DateTime.Now;
        Console.WriteLine($"Session ended for user {Username} at {EndTime}");
    }
}