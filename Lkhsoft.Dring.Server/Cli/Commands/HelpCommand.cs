using System.ComponentModel.Composition;
using System.Globalization;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     HELP command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Guest, AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "HELP")]
[ExportMetadata("CommandAlias", "?")]
public class HelpCommand : CommandBase
{
    /// <summary>
    /// Enumeration of all commands
    /// </summary>
    [ImportMany] private IEnumerable<Lazy<ICommand, ICommandMetadata>> _commandImports;

    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Error: Too many parameters. Usage: HELP <command>");
            return;
        }

        if (args.Length == 0)
        {
            ConsoleEventHandler.DisplayItems("HELP", _commandImports.Select(x => x.Metadata.CommandName));
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
        }
    }
}