#region

using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Shared.Core;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;

#endregion

namespace Lkhsoft.Dring.Shared.Cli;

/// <summary>
///     Common base class for all commands
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <summary>
    ///     Logger
    /// </summary>
    protected readonly IAppLogger _logger;

    /// <summary>
    ///     Current context accessor
    /// </summary>
    protected readonly IContextAccessor _contextAccessor;

    /// <summary>
    ///     Base constructor
    /// </summary>
    /// <param name="contextAccessor">Context of the current session</param>
    /// <param name="logger">Logger of the current context</param>
    public CommandBase(IContextAccessor contextAccessor, IAppLogger logger)
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public virtual void Execute(params string[] args)
    {
        Console.SetOut(_contextAccessor.GetTextWriter());
    }

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
    protected string ReadSecureOptions(string iv)
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