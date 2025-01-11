using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
/// LOGON command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "LOGON")]
public class LogonCommand : CommandBase
{
    ///<inheritdoc/>
    public override void Execute(params string[] args)
    {
        Console.WriteLine("LOGON command executed. You are now logged on.");
    }
}