using System.ComponentModel.Composition;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;

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
    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        base.Execute();
        Console.Clear();
    }
}