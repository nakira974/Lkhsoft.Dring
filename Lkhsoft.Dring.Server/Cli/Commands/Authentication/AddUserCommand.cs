#region

using System.ComponentModel.Composition;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using Lkhsoft.Dring.Shared.Cli;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;

#endregion

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     ADDUSER command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "ADDUSER")]
[ExportMetadata("CommandAlias", "")]
public class AddUserCommand : AuthenticationCommandBase
{
    /// <inheritdoc/>
    [ImportingConstructor]
    public AddUserCommand(IContextAccessor contextAccessor, IAppLogger logger) : base(contextAccessor, logger)
    {
    }

    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        base.Execute();
        if (args.Length < 1 || args.Length > 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine("Usage: ADDUSER <username>");
            return;
        }


        var username = args[0];
        if (await UserExistsAsync(username))
        {
            Console.WriteLine($"User ${username} already exists in the database");
            _logger.LogWarning($"USERADD command failed. User {username} already exists in the database.");
            return;
        }

        string? password;
        var iv = GetDeterministicIv(username);
        try
        {
            Console.WriteLine("\nEnter the password for the new user");
            password = ReadSecureOptions(iv);
        }
        catch (Exception ex)
        {
            return;
        }

        try
        {
            await InsertUserAsync(username, password, iv);
            Console.WriteLine("ADDUSER command executed. User added to the database.");
            _logger.LogInfo($"User {username} added to the database at {DateTime.Now}");
        }
        catch (Exception ex)
        {
            const string message = "Error while adding user to the database";
            Console.WriteLine(message);
            _logger.LogError(message);
        }
    }

    /// <summary>
    /// Generate a deterministic IV of 16 bytes length
    /// </summary>
    /// <param name="username">Username to be used to generate the initialization vector</param>
    /// <returns>A deterministic initialization vector based on the username</returns>
    private static string GetDeterministicIv(string username)
    {
        // Convertir l'username en tableau de bytes
        var usernameBytes = Encoding.UTF8.GetBytes(username);

        // Utiliser SHA256 pour générer un hash de l'username
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(usernameBytes);

        // Prendre les 8 premiers octets du hash
        var iv = new byte[8];
        Array.Copy(hash, iv, 8);

        // Convertir les 8 octets en une chaîne hexadécimale
        return BitConverter.ToString(iv).Replace("-", string.Empty);
    }

    /// <summary>
    /// Generate a random IV of 16 bytes length
    /// </summary>
    /// <returns>A random initialization vector</returns>
    private static string GetRandomIv()
    {
        var iv = new byte[8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);
        // Convertir les 8 octets en une chaîne hexadécimale
        return BitConverter.ToString(iv).Replace("-", string.Empty);
    }

    /// <summary>
    /// Create a new user in the database
    /// </summary>
    /// <param name="username">Username of the new user</param>
    /// <param name="encryptedPassword">Encrypted password</param>
    /// <param name="iv">Initialization vector</param>
    /// <returns>Une tâche asynchrone.</returns>
    private async Task InsertUserAsync(string username, string encryptedPassword, string iv)
    {
        const string query = @"
            INSERT INTO Users (Username, Password, IsConnected, IV)
            VALUES (@Username, @Password, 0, @IV);
        ";

        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction();
        await using var command = new SQLiteCommand(query, connection, transaction);
        try
        {
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", encryptedPassword);
            command.Parameters.AddWithValue("@IV", iv);

            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync(); // Validation de la transaction
        }
        catch
        {
            await transaction.RollbackAsync(); // Annulation de la transaction
            throw; // Relancer l'exception pour gestion en amont
        }
    }
}