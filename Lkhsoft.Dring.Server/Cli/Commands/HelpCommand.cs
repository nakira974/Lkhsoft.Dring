using System.ComponentModel.Composition;
using System.Globalization;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
/// HELP command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "HELP")]
public class HelpCommand : CommandBase
{
    ///<inheritdoc/>
    public override void Execute(params string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Error: Too many parameters. Usage: HELP <command>");
            return;
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: HELP <command>");
            return;
        }

        var command = args[0].ToUpper(CultureInfo.InvariantCulture);
        var helpFilePath = $"{command}_help.md";

        if (File.Exists(helpFilePath))
        {
            var helpContent = File.ReadAllText(helpFilePath);
            Console.WriteLine(helpContent);
        }
        else
        {
            Console.WriteLine($"Help file for command '{command}' not found.");
        }

        try
        {
            ReadOptions();
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }
}