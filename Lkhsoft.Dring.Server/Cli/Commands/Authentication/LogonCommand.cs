using System.ComponentModel.Composition;
using System.Data.SQLite;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     LOGON command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Guest)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "LOGON")]
[ExportMetadata("CommandAlias", "SU")]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class LogonCommand : AuthenticationCommandBase
{
    /// <summary>
    /// Session service
    /// </summary>
    [Import]
    private ISessionService _sessionService { get; set; }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public LogonCommand()
    {
    }

    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        if (args.Length < 1 || args.Length > 1 || String.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine("Usage: LOGON <username>");
            return;
        }

        var username = args[0];
        string? password;
        var iv = await GetUserIVAsync(username);
        try
        {
            password = ReadSecureOptions(iv);
        }
        catch (Exception ex)
        {
            return;
        }


        if (await AuthenticateUserAsync(username, password))
        {
            Console.WriteLine("LOGON command executed. You are now logged on.");
            // Create a session for the user
            var session = new Session(username);
            session.Start();

            // Add session to the database
            _sessionService.AddSession(session);

            // Update the IsConnected field in the Users table
            await UpdateUserConnectionStatusAsync(username, true);
            _logger.LogInfo($"User {username} logged on at {DateTime.Now}");
        }
        else
        {
            Console.WriteLine("Authentication failed. Please check your username and password.");
            _logger.LogError($"Authentication failed for user {username} at {DateTime.Now}");
        }
    }

    /// <summary>
    ///     Launch the user authentication process
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns></returns>
    private async Task<bool> AuthenticateUserAsync(string username, string? password)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Password", password);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// Get user initialization vector
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>IV of the current user</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> GetUserIVAsync(string username)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var query = "SELECT IV FROM Users WHERE Username = @Username";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader["IV"].ToString() ?? throw new InvalidOperationException($"User's {username} IV not found.");
        }
        else
        {
            throw new Exception("User not found.");
        }
    }
}