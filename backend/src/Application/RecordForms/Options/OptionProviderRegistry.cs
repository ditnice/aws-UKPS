using System.Linq.Expressions;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>The option providers, by key. Built once at startup.</summary>
internal sealed class OptionProviderRegistry
{
    private readonly Dictionary<string, IOptionProvider> _providers = new(StringComparer.Ordinal);

    public IEnumerable<IOptionProvider> Providers => _providers.Values;

    /// <summary>The registry with every provider the forms use.</summary>
    public static OptionProviderRegistry CreateDefault()
    {
        OptionProviderRegistry registry = new();
        MedicineOptionProviders.Register(registry);
        return registry;
    }

    public IOptionProvider? Find(string key) => _providers.GetValueOrDefault(key);

    /// <summary>
    /// The providers of a kind that offer <paramref name="source"/>, by key. A question with no
    /// <c>[FormOptions]</c> uses the only one.
    /// </summary>
    public IReadOnlyList<IOptionProvider> FindBySource(OptionProviderKind kind, Type source) =>
        _providers
            .Values.Where(p => p.Kind == kind && p.SourceType == source)
            .OrderBy(static p => p.Key, StringComparer.Ordinal)
            .ToArray();

    /// <summary>Adds a fixed list of enum members, offered in the order given.</summary>
    public void AddEnum<TEnum>(string key, params (TEnum Value, string Label)[] items)
        where TEnum : struct, Enum => Add(new EnumOptionProvider<TEnum>(key, items));

    /// <summary>Adds a flat list of reference data rows. The query and projection must translate to SQL.</summary>
    public void AddReference<TEntity>(
        string key,
        Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> query,
        Expression<Func<TEntity, ReferenceOptionRow>> projection
    )
        where TEntity : class =>
        Add(
            new ReferenceOptionProvider<TEntity>(
                key,
                OptionProviderKind.Reference,
                query,
                projection
            )
        );

    /// <summary>
    /// Adds two levels of reference data. The projection's <see cref="ReferenceOptionRow.ParentId"/>
    /// links children to parents, and the query's order is kept within each level.
    /// </summary>
    public void AddReferenceTree<TEntity>(
        string key,
        Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> query,
        Expression<Func<TEntity, ReferenceOptionRow>> projection
    )
        where TEntity : class =>
        Add(
            new ReferenceOptionProvider<TEntity>(
                key,
                OptionProviderKind.ReferenceTree,
                query,
                projection
            )
        );

    private void Add(IOptionProvider provider)
    {
        if (!_providers.TryAdd(provider.Key, provider))
        {
            throw new InvalidOperationException(
                $"An option provider with key '{provider.Key}' is already registered."
            );
        }
    }
}
