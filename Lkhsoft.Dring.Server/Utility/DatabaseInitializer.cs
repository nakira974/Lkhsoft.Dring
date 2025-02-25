#region

using System.Configuration;
using System.Data.SQLite;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Shared.Core;

#endregion

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
///     Database initializer
/// </summary>
public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly string _sqlFilePath = "database.sql";

    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <exception cref="InvalidOperationException">Datasource not found</exception>
    public DatabaseInitializer()
    {
        _connectionString = ConfigurationManager.ConnectionStrings["ServerDB"].ConnectionString
                            ?? throw new InvalidOperationException("Datasource connection string not found");
    }

    public async Task InitializeDatabaseAsync()
    {
        if (!File.Exists(_sqlFilePath)) throw new FileNotFoundException($"The SQL file {_sqlFilePath} does not exist");

        var sqlScript = await File.ReadAllTextAsync(_sqlFilePath);

        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SQLiteCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
        await InsertGuestIfNoExistsAsync();
    }
    
    /// <summary>
    /// Create a guest in the database if not exists
    /// </summary>
    /// <param name="username">Username of the new user</param>
    /// <param name="encryptedPassword">Encrypted password</param>
    /// <param name="iv">Initialization vector</param>
    private async Task InsertGuestIfNoExistsAsync()
    {
        const string existsQuery = @"
            SELECT 1 
            FROM Users
            WHERE Username = 'guest'
            LIMIT 1;
        ";

        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command1 = new SQLiteCommand(existsQuery, connection);
        var result = await command1.ExecuteScalarAsync();
        var exists = result is not null;

        if (!exists)
        {
            const string query = @"
            INSERT INTO Users (Username, Password, IsConnected, IV, Role)
            VALUES ('guest', '', 0, 'none', 'Guest');
        ";
            
            await using var transaction = connection.BeginTransaction();
            await using var command = new SQLiteCommand(query, connection, transaction);
            try
            {
                await command.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        else
        {
            Console.WriteLine("Guest user already exists");
        }
    }
}