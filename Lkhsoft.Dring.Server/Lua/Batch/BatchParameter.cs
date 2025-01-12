namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// Batch script parameter
/// </summary>
public class BatchParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Parameter type
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Parameter default value
    /// </summary>
    public string DefaultValue { get; set; }
}