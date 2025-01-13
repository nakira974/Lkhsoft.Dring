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
    /// Check if a user already exists in the database
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>True if the user already exists, otherwise false</returns>
    public async Task<bool> UserExistsAsync(string username)
    {
        const string query = @"
            SELECT 1 
            FROM Users
            WHERE Username = @Username
            LIMIT 1;
        ";

        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SQLiteCommand(query, connection);

        command.Parameters.AddWithValue("@Username", username);

        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }
}