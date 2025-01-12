using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     CLEAR command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "CLEAR")]
[ExportMetadata("CommandAlias", "")]
public class ClearCommand : CommandBase
{
    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        Console.Clear();
    }
}