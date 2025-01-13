namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
///     Server session model
/// </summary>
public class Session : IComparable<Session>
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="username">Username authenticated</param>
    public Session(string username)
    {
        Id = Guid.NewGuid();
        Username = username;
    }
    
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid Id { get; init; }

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

    ///<inheritdoc />
    public int CompareTo(Session? other)
    {
        return String.CompareOrdinal(Id.ToString(), other?.ToString());
    }
}