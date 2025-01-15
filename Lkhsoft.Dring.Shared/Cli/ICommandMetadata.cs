namespace Lkhsoft.Dring.Shared.Cli;

/// <summary>
///     CLI commands metadata
/// </summary>
public interface ICommandMetadata
{
    /// <summary>
    ///     Command name
    /// </summary>
    string CommandName { get; }

    /// <summary>
    ///     Command alias
    /// </summary>
    string CommandAlias { get; }
}