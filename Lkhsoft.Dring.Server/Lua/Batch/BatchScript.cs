namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// Represents a LUA batch script
/// </summary>
public sealed record BatchScript
{
    /// <summary>
    /// Script name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Script file path
    /// </summary>
    public string Script { get; set; }

    /// <summary>
    /// Script description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Script parameters
    /// </summary>
    public IEnumerable<BatchParameter> Parameters { get; set; }
}