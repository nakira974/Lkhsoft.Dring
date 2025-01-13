using System.ComponentModel.Composition;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     EXIT command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Guest, AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "EXIT")]
[ExportMetadata("CommandAlias", "QUIT")]
public class ExitCommand : CommandBase
{
    /// <summary>
    /// Session service
    /// </summary>
    [Import] private ISessionService _sessionService;

    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        Console.WriteLine("Exiting...");
        _sessionService.ClearAllSessions();
        Environment.Exit(0);
    }
}