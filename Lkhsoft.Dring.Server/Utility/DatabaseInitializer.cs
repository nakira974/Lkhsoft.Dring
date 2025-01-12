using System.Configuration;
using System.Data.SQLite;

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
    }
}