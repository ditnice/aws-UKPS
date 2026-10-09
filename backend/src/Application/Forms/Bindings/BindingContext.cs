using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Per-request state shared by bindings for one revision. Caches each content row so that
/// reading or writing several questions on the same table costs one query.
/// </summary>
internal sealed class BindingContext(AppDbContext dbContext, int revisionId)
{
    private readonly Dictionary<Type, object> _rows = [];

    public AppDbContext DbContext { get; } = dbContext;

    public int RevisionId { get; } = revisionId;

    /// <summary>
    /// Gets the revision's (tracked) row for <paramref name="row"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The row does not exist.</exception>
    public async Task<TEntity> GetRowAsync<TEntity>(
        ContentRow<TEntity> row,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        if (_rows.TryGetValue(typeof(TEntity), out var cached))
        {
            return (TEntity)cached;
        }

        var entity =
            await DbContext
                .Set<TEntity>()
                .SingleOrDefaultAsync(row.ForRevision(RevisionId), cancellationToken)
            ?? throw new InvalidOperationException(
                $"Revision {RevisionId} has no {typeof(TEntity).Name} row. It should have been created with the record."
            );

        _rows[typeof(TEntity)] = entity;
        return entity;
    }
}
