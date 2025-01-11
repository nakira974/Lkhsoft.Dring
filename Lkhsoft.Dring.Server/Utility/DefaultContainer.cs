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
                    var pluginsPath = ConfigurationManager.AppSettings["PluginsPath"] ?? throw new InvalidOperationException("Plugins path is missing");
                    var pluginsCatalog = new DirectoryCatalog(pluginsPath);
                    var catalog = new AggregateCatalog(pluginsCatalog);
                    
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
        /// Log a synchronized message
        /// </summary>
        private static void LogMessage(string message)
        {
            // Synchroniser l'accès à l'écriture dans le fichier de log
            lock (LockObject)
            {
                Logger.Info(message);
            }
        }

        /// <summary>
        /// Get an export from the container
        /// </summary>
        /// <typeparam name="T">Type of the export</typeparam>
        /// <returns>An instance of the given type</returns>
        public static T Get<T>()
        {
            try
            {
                LogMessage($"Récupération de l'export pour {typeof(T).Name}");

                var export = Container.GetExport<T>();

                if (export != null)
                {
                    LogMessage($"Export trouvé pour {typeof(T).Name}");
                }
                else
                {
                    LogMessage($"Aucun export trouvé pour {typeof(T).Name}");
                }

                return export.Value;
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la récupération de l'export pour {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }
    }