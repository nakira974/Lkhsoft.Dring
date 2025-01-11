using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Reflection;
using NLog;

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
/// Server container that uses MEF to load parts
/// </summary>
public static class DefaultContainer
    {
        // Logger NLog
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Verrou pour assurer la synchronisation de l'écriture dans le log
        private static readonly object LockObject = new object();

        // Instance unique du container MEF
        private static CompositionContainer _container;

        // Propriété statique pour accéder au container MEF
        private static CompositionContainer Container
        {
            get
            {
                if (_container is null)
                {
                    var catalogs = ConfigurePlugins("PluginsPath", "CommandsPath");
                    
                    var catalog = new AggregateCatalog(catalogs);
                    
                    // Ajouter les assemblages nécessaires au container
                    catalog.Catalogs.Add( new AssemblyCatalog(typeof(DefaultContainer).Assembly));
                    
                    _container = new CompositionContainer(catalog);

                    // Journaliser l'initialisation du container
                    LogMessage("Container MEF initialisé.");
                }

                return _container;
            }
        }

        /// <summary>
        /// Add an assembly to the container
        /// </summary>
        public static void ComposeParts(object part)
        {
            // Journaliser la composition de l'objet
            LogMessage($"Composition des parties pour {part.GetType().Name}");

            try
            {
                // Composition des parties avec MEF
                Container.ComposeParts(part);

                // Journaliser si la composition a réussi
                LogMessage($"Composition réussie pour {part.GetType().Name}");
            }
            catch (Exception ex)
            {
                // Enregistrer toute exception de composition
                LogMessage($"Erreur de composition pour {part.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get an export from the container
        /// </summary>
        /// <typeparam name="T">Type of the export</typeparam>
        /// <returns>An instance of the given type</returns>
        public static T? Get<T>() where T : class
        {
            try
            {
                var export = Container.GetExport<T>();
                if (export != null) return export.Value;
            }
            catch (Exception ex)
            {
                var type = typeof(T);
                throw new InvalidOperationException($"Could not find the export of type '{type.FullName}'", ex);
            }

            return null;
        }
        
        /// <summary>
        /// Returns catalog for plugins loaded from the configuration
        /// </summary>
        /// <param name="configKeys">Configuration keys</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Plugins path is invalid or missing</exception>
        private static IEnumerable<DirectoryCatalog> ConfigurePlugins(params string[] configKeys)
        {
            var catalogs = new List<DirectoryCatalog>(configKeys.Length);
            foreach (var configKey in configKeys)
            {
                var pluginsPath = ConfigurationManager.AppSettings[configKey];
                if(String.IsNullOrEmpty(pluginsPath)) 
                    throw new InvalidOperationException($"Plugins path for '{configKey}' is missing");
                try
                {
                    _ = Path.GetDirectoryName(pluginsPath);
                }
                catch
                {
                    throw new InvalidOperationException("Plugins path is invalid");
                }
                catalogs.Add(new DirectoryCatalog(pluginsPath));
            }

            return catalogs;
        }
            
        /// <summary>
        /// Log a synchronized message
        /// </summary>
        private static void LogMessage(string message)
        {
            // Synchroniser l'accès à l'écriture dans le fichier de log
            lock (LockObject)
            {
                var logger = Get<IAppLogger>();
                logger?.LogInfo(message);
            }
        }
    }