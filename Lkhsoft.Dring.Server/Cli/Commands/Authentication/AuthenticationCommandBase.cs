using System.Configuration;
using System.Data.SQLite;

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     Authentication base command
/// </summary>
public abstract class AuthenticationCommandBase : CommandBase
{
    /// <summary>
    /// </summary>
    private protected readonly string ConnectionString;


    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <exception cref="InvalidOperationException">Datasource not found</exception>
    protected AuthenticationCommandBase()
    {
        ConnectionString = ConfigurationManager.ConnectionStrings["ServerDB"].ConnectionString
                           ?? throw new InvalidOperationException("Datasource connection string not found");
    }

    /// <summary>
    ///     Update the status of the user's connection
    /// </summary>
    /// <param name="username">Username to update</param>
    /// <param name="isConnected">User status</param>
    private protected async Task UpdateUserConnectionStatusAsync(string username, bool isConnected)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var query = "UPDATE Users SET IsConnected = @IsConnected WHERE Username = @Username";
        await using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@IsConnected", isConnected ? 1 : 0);
        command.Parameters.AddWithValue("@Username", username);

        await command.ExecuteNonQueryAsync();
    }
}