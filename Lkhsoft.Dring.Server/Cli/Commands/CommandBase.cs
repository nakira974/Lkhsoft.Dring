using System.ComponentModel.Composition;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     Common base class for all commands
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <summary>
    ///     Logger
    /// </summary>
    [Import]
    private protected IAppLogger _logger { get; set; }

    /// <inheritdoc />
    public abstract void Execute(params string[] args);

    /// <summary>
    ///     Checks if an exit request has been made and throws an exception if so
    /// </summary>
    /// <exception cref="OperationCanceledException">Command has been canceled</exception>
    protected string ReadOptions(string prompt = "")
    {
        Console.Write(prompt);
        var input = ConsoleEventHandler.ReadLine();
        if (input == null)
        {
            Console.WriteLine("Exiting command");
            _logger.LogInfo($"Exiting {GetType()} command");
            throw new OperationCanceledException("Command has been cancelled");
        }

        return input;
    }
    
    /// <summary>
    /// Reads a secure line from the console
    /// </summary>
    /// <param name="iv">Initialization vector</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException">Command has been canceled</exception>
    protected string ReadSecureOptions(string iv )
    {
        var input = ConsoleEventHandler.ReadAndEncryptSecureLine(iv);
        if (input == null)
        {
            _logger.LogInfo($"Exiting {GetType()} command");
            throw new OperationCanceledException("Command has been cancelled");
        }

        return input;
    }
}