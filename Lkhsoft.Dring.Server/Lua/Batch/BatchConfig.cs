namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// Batch script configuration
/// </summary>
public class BatchConfig
{
    /// <summary>
    /// Scripts folder path
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// Registered batch scripts
    /// </summary>
    public IEnumerable<BatchScript> Batches { get; set; }
}