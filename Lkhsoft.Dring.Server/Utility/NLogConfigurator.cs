using NLog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lkhsoft.Dring.Server.Utility;

public static class NLogConfigurator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void ConfigureFromYaml(string yamlFilePath)
        {
            try
            {
                // Charger le fichier YAML
                var yaml = File.ReadAllText(yamlFilePath);
                var deserializer = new Deserializer();
                var config = deserializer.Deserialize<YamlConfiguration>(yaml);

                // Configurer les cibles NLog
                var fileTarget = new NLog.Targets.FileTarget
                {
                    FileName = config.Nlog.Targets[0].FileName,
                    Layout = config.Nlog.Targets[0].Layout,
                    ConcurrentWrites = config.Nlog.Targets[0].ConcurrentWrites,
                    OpenFileCacheSize = config.Nlog.Targets[0].OpenFileCacheSize,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Ajouter la règle pour logger
                var rule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, fileTarget);
                LogManager.Configuration.AddTarget("file", fileTarget);
                LogManager.Configuration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, fileTarget);

                // Appliquer la configuration
                LogManager.ReconfigExistingLoggers();
                Logger.Info("NLog configuré avec succès depuis YAML.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Erreur de configuration de NLog à partir de YAML.");
            }
        }

        // Classe de configuration pour le YAML
        public class YamlConfiguration
        {
            public NlogConfig Nlog { get; set; }
        }

        public class NlogConfig
        {
            public Target[] Targets { get; set; }
        }

        public class Target
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string FileName { get; set; }
            public string Layout { get; set; }
            public bool ConcurrentWrites { get; set; }
            public int OpenFileCacheSize { get; set; }
            public string Encoding { get; set; }
        }
    }