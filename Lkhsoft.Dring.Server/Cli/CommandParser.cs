using System.ComponentModel.Composition;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Server.Utility.Authentication;
using Lkhsoft.Dring.Server.Utility.Core;
using Lkhsoft.Dring.Server.Utility.Logger;

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
    ///     Logger
    /// </summary>
    [Import]
    private IAppLogger _logger { get; set; }

    /// <summary>
    ///     Session service
    /// </summary>
    [Import]
    private IContextAccessor _contextAccessor { get; set; }

    /// <summary>
    ///     Session container
    /// </summary>
    private readonly SessionContainer _sessionContainer;

    /// <summary>
    ///     Default constructor, initializes the container and commands
    /// </summary>
    public CommandParser()
    {
        _sessionContainer = new SessionContainer();
        _sessionContainer.ComposeParts(this);
        _contextAccessor.RegisterCliSession(this);

        _commands = (_commandImports ?? throw new InvalidOperationException("CLI commands import failed"))
            .SelectMany(import =>
            {
                var commands = new List<KeyValuePair<string, ICommand>>
                {
                    new KeyValuePair<string, ICommand>(import.Metadata.CommandName, import.Value)
                };

                // Si Alias n'est pas null ou vide, ajouter l'alias également
                if (!String.IsNullOrEmpty(import.Metadata.CommandAlias))
                {
                    commands.Add(new KeyValuePair<string, ICommand>(import.Metadata.CommandAlias, import.Value));
                }

                return commands;
            })
            .ToDictionary(command => command.Key, command => command.Value);
    }

    /// <summary>
    ///   Get the current CLI session
    /// </summary>
    /// <returns></returns>
    public Session GetUserSession()
    {
        return _contextAccessor.GetSession();
    }

    /// <summary>
    ///     Parse and execute commands
    /// </summary>
    /// <param name="input">Command to execute</param>
    public async void ParseAndExecute(string? input)
    {
        try
        {
            if (input is null) return;
            var parts = input.Trim().Split(' ');
            if (parts.Length == 0) return;

            var command = parts[0].ToUpper();
            if (_commands.TryGetValue(command, out var command1))
            {
                var args = parts.Skip(1).ToArray();
                var argumentsString = string.Join(" ", args);

                // Check authorization

                if (command1.GetType()
                        .GetCustomAttributes(typeof(AuthorizedCommandAttribute), true)
                        .FirstOrDefault() is AuthorizedCommandAttribute authorizedCommandAttribute)
                {
                    var userRole = await _contextAccessor.GetRole();
                    if (userRole is null || !Enum.TryParse(userRole, out AuthorizationType userAuthorizationType) ||
                        !authorizedCommandAttribute.Authorizations.Contains(userAuthorizationType))
                    {
                        Console.WriteLine("You do not have the required authorization to execute this command");
                        _logger.LogWarning(
                            $"Unauthorized command attempt: {command} by user {_contextAccessor.GetSession().Username}");
                        return;
                    }
                }

                command1.Execute(args);
                _logger.LogInfo($"Command {command} with args {argumentsString} has been executed");
            }
            else
            {
                Console.WriteLine("Command not found");
                _logger.LogWarning($"Command {command} not found");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while executing the command, ${e.Message}");
            _logger.LogError($"An error occurred while executing the command, ${e.Message}");
        }
    }
}