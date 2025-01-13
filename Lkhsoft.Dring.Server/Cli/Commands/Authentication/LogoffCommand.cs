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
        if (args.Length > 0)
        {
            Console.WriteLine("Usage: LOGOFF");
            return;
        }

        Session? session;
        try
        {
            session = _sessionService.GetSessionByUsername(Program.CurrentSession?.Username ??
                                                           throw new InvalidOperationException(
                                                               "No active session found"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            _logger.LogWarning($"LOGOFF attempt failed, ${ex.Message}");
            return;
        }

        if (session is null)
        {
            Console.WriteLine("No active session found.");
            return;
        }

        _logger.LogInfo($"User {session.Username} logged off at {DateTime.Now}");
        _sessionService.RemoveSession(session);
        _logger.LogInfo($"Session for {session.Username} removed");

        Console.WriteLine("LOGOFF command executed. You are now logged off.");
    }
}