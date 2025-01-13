using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.SQLite;

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
/// Session service implementation
/// </summary>
[Export(typeof(ISessionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SessionService : ISessionService
{
    /// <summary>
    /// App logger
    /// </summary>
    [Import]
    private IAppLogger _logger { get; set; }

    /// <summary>
    /// List of sessions for the current instance
    /// </summary>
    private readonly ISet<Session> _sessions;

    /// <summary>
    /// Database connection string
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// Session callback
    /// </summary>
    private Action<Session>? _sessionCallback;

    /// <summary>
    /// Default constructor
    /// </summary>
    public SessionService()
    {
        _sessions = new HashSet<Session>();
        _connectionString = ConfigurationManager.ConnectionStrings["ServerDB"].ConnectionString;
    }

    /// <inheritdoc />
    public void RegisterSessionCallback(Action<Session> callback)
    {
        _sessionCallback = callback;
    }

    /// <inheritdoc />
    public void TriggerSessionCallback(Session session)
    {
        _sessionCallback?.Invoke(session);
    }

    ///<inheritdoc />
    public Session? GetSessionByUsername(string username)
    {
        return _sessions.FirstOrDefault(s => s.Username == username);
    }

    ///<inheritdoc />
    public async void AddSession(Session session)
    {
        try
        {
            if (_sessions.Add(session))
            {
                await AddSessionToDatabaseAsync(session);
                _logger.LogTrace($"Session {session.Id} created at {DateTime.Now}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Session {session.Id} could not be added to the database. {e.Message}");
        }
    }

    ///<inheritdoc />
    public async void RemoveSession(Session session)
    {
        try
        {
            if (_sessions.Remove(session))
            {
                await UpdateSessionEndTimeAsync(session.Id.ToString());
                _logger.LogTrace($"Session {session.Id} removed at {DateTime.Now}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Session {session.Id} could not be removed to the database. {e.Message}");
        }
    }

    ///<inheritdoc />
    public void ClearAllSessions()
    {
        foreach (var session in _sessions)
        {
            RemoveSession(session);
        }
    }

    /// <summary>
    /// Add the newly created session to the database
    /// </summary>
    /// <param name="session">Created session</param>
    private async Task AddSessionToDatabaseAsync(Session session)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var query = "INSERT INTO Sessions (SessionId, Username, StartTime) VALUES (@SessionId, @Username, @StartTime)";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@SessionId", session.Id.ToString());
        command.Parameters.AddWithValue("@Username", session.Username);
        command.Parameters.AddWithValue("@StartTime", session.StartTime);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Update the session end time in the database
    /// </summary>
    /// <param name="id">Session to be logged off/param"</param>
    private async Task UpdateSessionEndTimeAsync(string id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Sessions SET EndTime = @EndTime WHERE SessionId = @SessionId AND EndTime IS NULL";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@EndTime", DateTime.Now);
        command.Parameters.AddWithValue("@SessionId", id);

        await command.ExecuteNonQueryAsync();
    }

    ///<inheritdoc />
    public async Task<string?> GetUserRoleAsync(string username)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT Role FROM Users WHERE Username = @Username";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        var role = await command.ExecuteScalarAsync() as string;
        return role;
    }
}