#region

using System.ComponentModel.Composition;
using System.Security;
using Lkhsoft.Dring.Shared.Cli;
using Lkhsoft.Dring.Shared.Core;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;

#endregion

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
    [ImportingConstructor]
    public ExitCommand([Import] IContextAccessor contextAccessor, [Import] IAppLogger logger) : base(contextAccessor,
        logger)
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