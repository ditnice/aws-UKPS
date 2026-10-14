using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Per-request state shared by bindings for one revision. Caches each content row (or its
/// absence) so that reading or writing several questions on the same table costs one query.
/// </summary>
internal sealed class BindingContext(AppDbContext dbContext, int revisionId)
{
    private readonly Dictionary<Type, object?> _rows = [];

    public AppDbContext DbContext { get; } = dbContext;

    public int RevisionId { get; } = revisionId;

    /// <summary>
    /// Gets the revision's (tracked) row for <paramref name="row"/>, or <c>null</c> if it has
    /// not been created yet.
    /// </summary>
    public async Task<TEntity?> FindRowAsync<TEntity>(
        ContentRow<TEntity> row,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        if (_rows.TryGetValue(typeof(TEntity), out var cached))
        {
            return (TEntity?)cached;
        }

        var entity = await DbContext
            .Set<TEntity>()
            .SingleOrDefaultAsync(row.ForRevision(RevisionId), cancellationToken);
        _rows[typeof(TEntity)] = entity;
        return entity;
    }

    /// <summary>
    /// Gets the revision's row for <paramref name="row"/>, creating (and tracking) it if it does
    /// not exist yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The row does not exist and this kind of row cannot be created by the form.
    /// </exception>
    public async Task<TEntity> GetOrCreateRowAsync<TEntity>(
        ContentRow<TEntity> row,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        if (await FindRowAsync(row, cancellationToken) is { } existing)
        {
            return existing;
        }

        var created =
            row.Create(RevisionId)
            ?? throw new InvalidOperationException(
                $"Revision {RevisionId} has no {typeof(TEntity).Name} row, and the form cannot create one."
            );
        DbContext.Add(created);
        _rows[typeof(TEntity)] = created;
        return created;
    }

    /// <summary>Whether <paramref name="entity"/> was created in this request and is not saved yet.</summary>
    public bool IsNew(object entity) => DbContext.Entry(entity).State == EntityState.Added;
}
