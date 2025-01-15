using System.ComponentModel.Composition;
using System.Security;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Server.Utility.Authentication;
using Lkhsoft.Dring.Server.Utility.Core;

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
    private readonly ISessionService _sessionService;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ExitCommand()
    {
        _sessionService = DefaultContainer.Get<ISessionService>() ??
                          throw new SecurityException("Global session service not found");
    }

    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        base.Execute();
        Console.WriteLine("Exiting...");
        _sessionService.ClearAllSessions();
        Environment.Exit(0);
    }
}