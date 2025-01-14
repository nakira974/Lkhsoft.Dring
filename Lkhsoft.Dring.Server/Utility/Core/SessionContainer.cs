using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Lkhsoft.Dring.Server.Cli;
using Lkhsoft.Dring.Server.Utility.Authentication;

namespace Lkhsoft.Dring.Server.Utility.Core;

/// <summary>
///     Session container isolating the commands and services
/// </summary>
public class SessionContainer
{
    /// <summary>
    ///     Session container
    /// </summary>
    private CompositionContainer _container;

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="catalogs">Catalogs to be inserted in the session container</param>
    public SessionContainer(params ComposablePartCatalog[]? catalogs)
    {
        var plugins = DefaultContainer.ConfigurePlugins("PluginsPath", "CommandsPath");
        var aggregateCatalog = new AggregateCatalog(catalogs);
        foreach (var directoryCatalog in plugins) aggregateCatalog.Catalogs.Add(directoryCatalog);
        var filteredCatalog = new FilteredCatalog(aggregateCatalog, typeof(ICommand), typeof(ISessionService));
        _container = new CompositionContainer(filteredCatalog);
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public SessionContainer()
    {
        var catalogs = DefaultContainer.ConfigurePlugins("PluginsPath", "CommandsPath");

        var catalog = new AggregateCatalog(catalogs);

        catalog.Catalogs.Add(new AssemblyCatalog(typeof(SessionContainer).Assembly));

        _container = new CompositionContainer(catalog);
    }

    /// <summary>
    ///     Add an assembly to the container
    /// </summary>
    public void ComposeParts(object part)
    {
        try
        {
            _container.ComposeParts(part);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not compose the parts", ex);
        }
    }

    /// <summary>
    ///     Retrieve a service from the container
    /// </summary>
    /// <typeparam name="T">Type to be found</typeparam>
    /// <returns>An instance of the given type</returns>
    public T? Get<T>() where T : class
    {
        try
        {
            var export = _container.GetExport<T>();
            if (export is not null) return export.Value;
        }
        catch (Exception ex)
        {
            var type = typeof(T);
            throw new InvalidOperationException($"Could not find the export of type '{type.FullName}'", ex);
        }

        return null;
    }

    /// <summary>
    ///     Retrieve many services from the container
    /// </summary>
    /// <typeparam name="T">Type to be found</typeparam>
    /// <returns>Instances of the given type</returns>
    public IEnumerable<T> GetMany<T>() where T : class
    {
        try
        {
            var exports = _container.GetExportedValues<T>();
            var enumerable = exports as T[] ?? exports.ToArray();
            if (enumerable.Length != 0) return enumerable;
        }
        catch (Exception ex)
        {
            var type = typeof(T);
            throw new InvalidOperationException($"Could not find the export of type '{type.FullName}'", ex);
        }

        return new List<T>();
    }
}