using System.ComponentModel.Composition;
using System.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// Batch script configuration loader
/// </summary>
[Export(typeof(BatchConfigLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BatchConfigLoader
{
    /// <summary>
    /// Loaded configuration
    /// </summary>
    public readonly BatchConfig BatchConfig;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public BatchConfigLoader()
    {
        BatchConfig = LoadBatchConfig().BatchConfig;
        if (!String.IsNullOrEmpty(BatchConfig.Path) && (BatchConfig.Path.EndsWith('/') || BatchConfig.Path.EndsWith('\\')))
        {
            BatchConfig.Path =  BatchConfig.Path.Substring(0, BatchConfig.Path.Length - 1);
        }
    }
    
    /// <summary>
    /// Load the batch configuration from the specified file
    /// </summary>
    /// <returns>The deserialized batch script configuration</returns>
    private static YamlConfiguration LoadBatchConfig()
    {
        var filePath = ConfigurationManager.AppSettings["BatchConfigPath"] ?? throw new InvalidOperationException("Batch configuration file path not found in the configuration file");
        var deserializer = new DeserializerBuilder() .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        using var reader = new StreamReader(filePath);
        return deserializer.Deserialize<YamlConfiguration>(reader);
    }
    
    /// <summary>
    /// Yaml model of the batch configuration
    /// </summary>
    private class YamlConfiguration
    {
        /// <summary>
        /// Batch configuration
        /// </summary>
        public BatchConfig BatchConfig { get; set; }
    }
}