using System.ComponentModel.Composition;
using System.Globalization;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Server.Utility.Authentication;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     HELP command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.Guest, AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "HELP")]
[ExportMetadata("CommandAlias", "?")]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HelpCommand : CommandBase
{
    /// <summary>
    ///     Enumeration of all commands
    /// </summary>
    private readonly IEnumerable<Lazy<ICommand, ICommandMetadata>> _commandImports;

    /// <summary>
    ///     Session service
    /// </summary>
    private readonly IContextAccessor _contextAccessor;

    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="contextAccessor">IContextAccessor part</param>
    /// <param name="commandImports"> ICommand parts</param>
    [ImportingConstructor]
    public HelpCommand(
        [Import] IContextAccessor contextAccessor,
        [ImportMany] IEnumerable<Lazy<ICommand, ICommandMetadata>> commandImports
    )
    {
        _contextAccessor = contextAccessor;
        _commandImports = commandImports;
    }

    /// <inheritdoc />
    public override async void Execute(params string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Error: Too many parameters. Usage: HELP <command>");
            return;
        }

        ICollection<Lazy<ICommand, ICommandMetadata>> authorizedCommands = new List<Lazy<ICommand, ICommandMetadata>>();
        if (authorizedCommands == null)
            throw new ArgumentNullException(nameof(authorizedCommands));

        foreach (var currentCommand in _commandImports)
        {
            if (currentCommand.Value.GetType()
                    .GetCustomAttributes(typeof(AuthorizedCommandAttribute), true)
                    .FirstOrDefault() is not AuthorizedCommandAttribute authorizedCommandAttribute) continue;

            var userRole = await _contextAccessor.GetRole();
            if (userRole is null || !Enum.TryParse(userRole, out AuthorizationType userAuthorizationType) ||
                !authorizedCommandAttribute.Authorizations.Contains(userAuthorizationType))
            {
                continue;
            }

            authorizedCommands.Add(currentCommand);
        }


        if (args.Length == 0)
        {
            ConsoleEventHandler.DisplayItems("HELP", authorizedCommands.Select(x => x.Metadata.CommandName));
            return;
        }

        var command = args[0].ToUpper(CultureInfo.InvariantCulture);
        var helpFilePath = $"{command}_help.md";

        if (File.Exists(helpFilePath))
        {
            var helpContent = await File.ReadAllTextAsync(helpFilePath);
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