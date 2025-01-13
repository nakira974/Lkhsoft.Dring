using System.ComponentModel.Composition;
using System.Data.SQLite;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli.Commands.Authentication;

/// <summary>
///     LOGOFF command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "LOGOFF")]
[ExportMetadata("CommandAlias", "LOGOUT")]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class LogoffCommand : AuthenticationCommandBase
{
    /// <summary>
    /// Session service
    /// </summary>
    [Import]
    private ISessionService _sessionService { get; set; }

    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: LOGOFF <username>");
            return;
        }

        var username = args[0];

        // Récupérer la session active pour l'utilisateur
        var session = _sessionService.GetSessionByUsername(username);
        if (session == null)
        {
            Console.WriteLine($"No active session found for user {username}");
            return;
        }

        _logger.LogInfo($"User {username} logged off at {DateTime.Now}");

        // Mettre à jour la session en la retirant
        _sessionService.RemoveSession(session);
        _logger.LogInfo($"Session for {username} removed");

        Console.WriteLine("LOGOFF command executed. You are now logged off.");
    }
}