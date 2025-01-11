namespace Lkhsoft.Dring.Server.Cli;

/// <summary>
/// CLI commands metadata
/// </summary>
public interface ICommandMetadata
{
    /// <summary>
    /// Command name
    /// </summary>
    string CommandName { get; }
}