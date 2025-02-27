#region
using System.ComponentModel.Composition;
using Lkhsoft.Dring.Shared.Cli;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;
#endregion

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     CLEAR command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Guest, AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "CLEAR")]
[ExportMetadata("CommandAlias", "")]
public class ClearCommand : CommandBase
{
    /// <inheritdoc/>
    [ImportingConstructor]
    public ClearCommand([Import] IContextAccessor contextAccessor, [Import] IAppLogger logger) : base(contextAccessor,
        logger)
    {
    }

    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        base.Execute();
        Console.Clear();
    }
}