using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     EXIT command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "EXIT")]
public class ExitCommand : CommandBase
{
    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        Console.WriteLine("Exiting...");
        Environment.Exit(0);
    }
}