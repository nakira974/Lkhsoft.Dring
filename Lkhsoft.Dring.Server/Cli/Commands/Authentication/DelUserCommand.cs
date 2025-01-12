using System.ComponentModel.Composition;
using System.Data.SQLite;

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     DELUSER command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "DELUSER")]
public class DelUserCommand : AuthenticationCommandBase
{
    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        if (args.Length < 1 || args.Length > 1 || String.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine("Usage: DELUSER <username>");
            return;
        }

        var username = args[0];
        if (!await UserExistsAsync(username))
        {
            Console.WriteLine("User does not exist in the database");
            _logger.LogWarning($"DELUSER command failed. User {username} does not exist in the database");
            return;
        }

        if (!await DeleteUserAsync(username))
        {
            Console.WriteLine("Error deleting user");
            _logger.LogError($"DELUSER command failed. User {username} could not be deleted");
            return;
        }

        Console.WriteLine("DELUSER command executed. User deleted successfully");
        _logger.LogInfo($"User {username} deleted at {DateTime.Now}");
    }

    /// <summary>
    /// Removes a user from the database
    /// </summary>
    /// <param name="username">Username of the user to delete</param>
    /// <returns>True if the user was deleted, false otherwise</returns>
    public async Task<bool> DeleteUserAsync(string username)
    {
        const string query = @"
            DELETE FROM Users
            WHERE Username = @Username;
        ";

        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}