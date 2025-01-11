using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Lkhsoft.Dring.Server.Cli;

/// <summary>
/// Command parser used in the CLI
/// </summary>
public class CommandParser
{
    /// <summary>
    /// Container for MEF exports of commands
    /// </summary>
    private readonly CompositionContainer _container;
    
    /// <summary>
    /// Server's commands mapped by their name
    /// </summary>
    private readonly Dictionary<string, ICommand> _commands;

    /// <summary>
    /// Import of commands
    /// </summary>
    [ImportMany]
    private IEnumerable<Lazy<ICommand, ICommandMetadata>> _commandImports;

    /// <summary>
    /// Default constructor, initializes the container and commands
    /// </summary>
    public CommandParser()
    {
        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new AssemblyCatalog(typeof(CommandParser).Assembly));
        _container = new CompositionContainer(catalog);

        _container.ComposeParts(this);

        _commands = (_commandImports ?? throw new InvalidOperationException("CLI commands import failed")).ToDictionary(
            import => import.Metadata.CommandName,
            import => import.Value
        );
    }

    /// <summary>
    /// Parse and execute commands
    /// </summary>
    /// <param name="input">Command to execute</param>
    public void ParseAndExecute(string? input)
    {
        if (input != null)
        {
            var parts = input.Trim().Split(' ');
            if (parts.Length == 0) return;

            var command = parts[0].ToUpper();
            if (_commands.TryGetValue(command, out var command1))
            {
                var args = parts.Skip(1).ToArray();
                command1.Execute(args);
            }
            else
            {
                Console.WriteLine("Command not found.");
            }
        }
    }
}