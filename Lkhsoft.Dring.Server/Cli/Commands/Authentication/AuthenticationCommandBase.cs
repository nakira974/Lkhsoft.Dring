#region

using System.Configuration;
using System.Data.SQLite;
using Lkhsoft.Dring.Shared.Cli;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;

#endregion

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     Authentication base command
/// </summary>
public abstract class AuthenticationCommandBase : CommandBase
{
    /// <summary>
    ///     Database connection string
    /// </summary>
    private protected readonly string ConnectionString;


    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <exception cref="InvalidOperationException">Datasource not found</exception>
    protected AuthenticationCommandBase(IContextAccessor contextAccessor, IAppLogger logger) : base(contextAccessor,
        logger)
    {
        ConnectionString = ConfigurationManager.ConnectionStrings["ServerDB"].ConnectionString
                           ?? throw new InvalidOperationException("Datasource connection string not found");
    }

    /// <summary>
    ///     Check if a user already exists in the database
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>True if the user already exists, otherwise false</returns>
    protected async Task<bool> UserExistsAsync(string username)
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