using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lkhsoft.Dring.Server.Utility.Logger;

/// <summary>
///     Configurator for NLog
/// </summary>
public static class NLogConfigurator
{
    /// <summary>
    ///     Logger NLog
    /// </summary>
    private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Configure NLog from a YAML file
    /// </summary>
    /// <param name="yamlFilePath">Yaml config path</param>
    public static void ConfigureFromYaml(string yamlFilePath)
    {
        try
        {
            // Vérifie si le fichier YAML existe
            if (!File.Exists(yamlFilePath))
                // Créer un fichier YAML par défaut
                CreateDefaultYamlFile(yamlFilePath);

            // Charge le fichier YAML
            var yaml = File.ReadAllText(yamlFilePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var config = deserializer.Deserialize<YamlConfiguration>(yaml);

            // Configure les cibles NLog
            var fileTarget = new FileTarget
            {
                Name = config.Nlog.Targets[0].Name,
                FileName = config.Nlog.Targets[0].FileName,
                Layout = config.Nlog.Targets[0].Layout,
                ConcurrentWrites = config.Nlog.Targets[0].ConcurrentWrites,
                OpenFileCacheSize = config.Nlog.Targets[0].OpenFileCacheSize,
                KeepFileOpen = config.Nlog.Targets[0].KeepFileOpen,
                Encoding = Encoding.UTF8,
                ArchiveFileName = config.Nlog.Targets[0].ArchiveFileName,
                MaxArchiveFiles = config.Nlog.Targets[0].MaxArchiveFiles,
                ArchiveAboveSize = config.Nlog.Targets[0].ArchiveAboveSize
            };

            var nlogConfig = new LoggingConfiguration();

            // Ajouter la règle pour logger
            foreach (var ruleConfig in config.Nlog.Rules)
            {
                var rule = new LoggingRule(ruleConfig.Logger, LogLevel.FromString(ruleConfig.MinLevel), fileTarget);
                nlogConfig.AddRule(rule);
            }

            LogManager.Configuration = nlogConfig;
            LogManager.Configuration.AddTarget("file", fileTarget);

            // Appliquer la configuration
            LogManager.ReconfigExistingLoggers();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not create NLog configuration from YAML file", ex);
        }
    }

    /// <summary>
    ///     Create a default YAML file with NLog configuration if not exists
    /// </summary>
    /// <param name="yamlFilePath"></param>
    private static void CreateDefaultYamlFile(string yamlFilePath)
    {
        var defaultConfig = new YamlConfiguration
        {
            Nlog = new NlogConfig
            {
                Targets =
                [
                    new Target
                    {
                        FileName = "${basedir}/logs/${shortdate}.log",
                        Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
                        ConcurrentWrites = true,
                        OpenFileCacheSize = 10
                    }
                ]
            }
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(defaultConfig);

        File.WriteAllText(yamlFilePath, yaml);
        Logger.Info($"Fichier YAML par défaut créé à l'emplacement : {yamlFilePath}");
    }

    /// <summary>
    ///     Configuration model for NLog from YAML
    /// </summary>
    private class YamlConfiguration
    {
        public NlogConfig Nlog { get; set; }
    }

    /// <summary>
    ///     Configuration model for NLog
    /// </summary>
    private class NlogConfig
    {
        public Target[] Targets { get; set; }
        public Rule[] Rules { get; set; }
    }

    /// <summary>
    ///     Configuration model for NLog target
    /// </summary>
    private class Target
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Layout { get; set; }
        public bool ConcurrentWrites { get; set; }
        public int OpenFileCacheSize { get; set; }
        public bool KeepFileOpen { get; set; }
        public string Encoding { get; set; }
        public string ArchiveFileName { get; set; }
        public int MaxArchiveFiles { get; set; }
        public long ArchiveAboveSize { get; set; }
    }

    /// <summary>
    ///     Configuration model for NLog rule
    /// </summary>
    private class Rule
    {
        public string Logger { get; set; }
        public string MinLevel { get; set; }
        public string WriteTo { get; set; }
    }
}