using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>
/// Reference data options from an EF query and projection, run on every request. Flat providers
/// offer every row the query returns; tree providers offer two levels, parents with children.
/// </summary>
internal sealed class ReferenceOptionProvider<TEntity> : IOptionProvider
    where TEntity : class
{
    private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _query;
    private readonly Expression<Func<TEntity, ReferenceOptionRow>> _projection;

    public ReferenceOptionProvider(
        string key,
        OptionProviderKind kind,
        Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> query,
        Expression<Func<TEntity, ReferenceOptionRow>> projection
    )
    {
        Key = key;
        Kind = kind;
        _query = query.Compile();
        _projection = projection;
        Description =
            $"{kind}:{typeof(TEntity).Name}:{ExpressionDescription.Describe(query)}:{ExpressionDescription.Describe(projection)}";
    }

    public string Key { get; }
    public OptionProviderKind Kind { get; }
    public Type SourceType => typeof(TEntity);
    public string Description { get; }

    public async Task<IReadOnlyList<FormOption>> GetOptionsAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    )
    {
        List<ReferenceOptionRow> rows = await _query(dbContext.Set<TEntity>().AsNoTracking())
            .Select(_projection)
            .ToListAsync(cancellationToken);
        HashSet<int> saved = savedValues
            .Select(static v => int.TryParse(v, CultureInfo.InvariantCulture, out int id) ? id : 0)
            .ToHashSet();

        return Kind == OptionProviderKind.ReferenceTree
            ? BuildTree(rows, saved)
            : rows.Where(row => IsOffered(row, saved)).Select(ToOption).ToArray();
    }

    private static FormOption[] BuildTree(List<ReferenceOptionRow> rows, HashSet<int> saved)
    {
        HashSet<int> parentIds = rows.Where(static r => r.ParentId is null)
            .Select(static r => r.Id)
            .ToHashSet();
        if (rows.Any(r => r.ParentId is int parentId && !parentIds.Contains(parentId)))
        {
            throw new InvalidOperationException(
                $"Option provider for {typeof(TEntity).Name} returned rows more than two levels deep, or children without their parent."
            );
        }

        ILookup<int?, ReferenceOptionRow> children = rows.Where(static r => r.ParentId is not null)
            .ToLookup(static r => r.ParentId);

        List<FormOption> options = [];
        foreach (ReferenceOptionRow parent in rows.Where(static r => r.ParentId is null))
        {
            // Children of an archived parent are only offered if they are already saved.
            FormOption[] offeredChildren = children[parent.Id]
                .Where(child =>
                    parent.IsArchived ? saved.Contains(child.Id) : IsOffered(child, saved)
                )
                .Select(ToOption)
                .ToArray();
            if (
                IsOffered(parent, saved)
                || offeredChildren.Any(c =>
                    saved.Contains(int.Parse(c.Value, CultureInfo.InvariantCulture))
                )
            )
            {
                options.Add(ToOption(parent) with { Children = offeredChildren });
            }
        }

        return [.. options];
    }

    private static bool IsOffered(ReferenceOptionRow row, HashSet<int> saved) =>
        !row.IsArchived || saved.Contains(row.Id);

    private static FormOption ToOption(ReferenceOptionRow row) =>
        new(row.Id.ToString(CultureInfo.InvariantCulture), row.Label);
}
