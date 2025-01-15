namespace Lkhsoft.Dring.Shared.Cli;

/// <summary>
///     Definition of a command used in the CLI
/// </summary>
public interface ICommand
{
    /// <summary>
    /// </summary>
    /// <param name="args">Command arguments</param>
    void Execute(params string[] args);
}