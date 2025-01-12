using System.ComponentModel.Composition;
using System.Data.SQLite;

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     LOGOFF command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "LOGOFF")]
public class LogoffCommand : AuthenticationCommandBase
{
    private readonly string _connectionString = "Data Source=users.db;Version=3;";

    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: LOGOFF <username>");
            return;
        }

        var username = args[0];

        // Update the IsConnected field in the Users table
        await UpdateUserConnectionStatusAsync(username, false);
        _logger.LogInfo($"User {username} logged off at {DateTime.Now}");

        // Update the session end time in the Sessions table
        await UpdateSessionEndTimeAsync(username);

        Console.WriteLine("LOGOFF command executed. You are now logged off.");
    }

    /// <summary>
    ///     Update the session end time in the database
    /// </summary>
    /// <param name="username">Disconnected user</param>
    private async Task UpdateSessionEndTimeAsync(string username)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE Sessions SET EndTime = @EndTime WHERE Username = @Username AND EndTime IS NULL";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@EndTime", DateTime.Now);
        command.Parameters.AddWithValue("@Username", username);

        await command.ExecuteNonQueryAsync();
    }
}