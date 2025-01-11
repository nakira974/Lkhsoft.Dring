namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
/// Common base class for all commands
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <inheritdoc/>
    public abstract void Execute(params string[] args);

    /// <summary>
    /// Checks if an exit request has been made and throws an exception if so
    /// </summary>
    /// <exception cref="OperationCanceledException"></exception>
    protected string ReadOptions(string prompt = "")
    {
        Console.Write(prompt);
        var input = ConsoleEventHandler.ReadLine();
        if (input == null)
        {
            Console.WriteLine("Exiting command.");
            throw new OperationCanceledException("Command has been cancelled.");
        }
        return input;
    }
}