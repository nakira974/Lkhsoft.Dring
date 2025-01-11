using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
/// LOGOFF command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "LOGOFF")]
public class LogoffCommand : CommandBase
{
    ///<inheritdoc/>
    public override void Execute(params string[] args)
    {
        Console.WriteLine("LOGOFF command executed. You are now logged off.");
    }
}