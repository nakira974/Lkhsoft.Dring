using System.ComponentModel.Composition;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli;

/// <summary>
///     Command parser used in the CLI
/// </summary>
public class CommandParser
{
    /// <summary>
    ///     Server's commands mapped by their name
    /// </summary>
    private readonly Dictionary<string, ICommand> _commands;

    /// <summary>
    ///     Import of commands
    /// </summary>
    [ImportMany] private IEnumerable<Lazy<ICommand, ICommandMetadata>> _commandImports;

    /// <summary>
    ///     Default constructor, initializes the container and commands
    /// </summary>
    public CommandParser()
    {
        DefaultContainer.ComposeParts(this);

        _commands = (_commandImports ?? throw new InvalidOperationException("CLI commands import failed")).ToDictionary(
            import => import.Metadata.CommandName,
            import => import.Value
        );
    }

    /// <summary>
    ///     Logger
    /// </summary>
    [Import]
    private IAppLogger _logger { get; set; }

    /// <summary>
    ///     Parse and execute commands
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
                var argumentsString = string.Join(" ", args);
                command1.Execute(args);
                _logger.LogInfo($"Command {command} with args {argumentsString} has been executed");
            }
            else
            {
                Console.WriteLine("Command not found.");
                _logger.LogWarning($"Command {command} not found");
            }
        }
    }
}