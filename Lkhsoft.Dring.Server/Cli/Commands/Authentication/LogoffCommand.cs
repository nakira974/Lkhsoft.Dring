using System.ComponentModel.Composition;
using System.Data.SQLite;
using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Server.Utility.Authentication;

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
    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        base.Execute();
        if (args.Length > 0)
        {
            Console.WriteLine("Usage: LOGOFF");
            return;
        }

        Session? session;
        try
        {
            session = _contextAccessor.GetSession();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            _logger.LogWarning($"LOGOFF attempt failed, ${ex.Message}");
            return;
        }

        _logger.LogInfo($"User {session.Username} logged off at {DateTime.Now}");
        _contextAccessor.Unregister();
        _logger.LogInfo($"Session for {session.Username} removed");

        Console.WriteLine("LOGOFF command executed. You are now logged off.");
    }
}