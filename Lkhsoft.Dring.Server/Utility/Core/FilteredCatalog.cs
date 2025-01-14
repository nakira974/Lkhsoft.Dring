using System.ComponentModel.Composition.Primitives;

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
///     Filtered catalog for selecting parts based on their types
/// </summary>
public class FilteredCatalog : ComposablePartCatalog
{
    /// <summary>
    ///     Catalog to be filtered
    /// </summary>
    private readonly ComposablePartCatalog _innerCatalog;

    /// <summary>
    ///     Types to filter the catalog
    /// </summary>
    private readonly IEnumerable<Type> _filterTypes;

    /// <summary>
    ///    Base constructor
    /// </summary>
    /// <param name="innerCatalog">Catalog to be filtered</param>
    /// <param name="filterTypes">Types to filter the catalog</param>
    public FilteredCatalog(ComposablePartCatalog innerCatalog, params Type[] filterTypes)
    {
        _innerCatalog = innerCatalog;
        _filterTypes = filterTypes;
    }

    /// <summary>
    ///     Catalog filtered parts
    /// </summary>
    public override IQueryable<ComposablePartDefinition> Parts
    {
        get
        {
            return _innerCatalog.Parts.Where(part =>
                part.ExportDefinitions.Any(export => _filterTypes.Contains(Type.GetType(export.ContractName))));
        }
    }
}