using System.Linq.Expressions;

namespace UKPS.Api.Application.Forms.Bindings;

/// <inheritdoc cref="IContentRow"/>
/// <remarks>
/// A row with a <c>create</c> factory is created by the first save that writes to it; until
/// then its questions read as unanswered. The factory must set every required column, which
/// the compiler enforces for <c>required</c> members. A row without a factory must already
/// exist (for example, because record creation inserts it); a missing one is an error.
/// </remarks>
internal sealed class ContentRow<TEntity>(
    Expression<Func<TEntity, int>> revisionId,
    Expression<Func<TEntity, int>> id,
    Func<int, TEntity>? create = null
) : IContentRow
    where TEntity : class
{
    private readonly Func<TEntity, int> _getId = id.Compile();

    public Type EntityType => typeof(TEntity);

    public int GetId(TEntity entity) => _getId(entity);

    /// <summary>Creates a new row for a revision, or <c>null</c> if rows cannot be created.</summary>
    public TEntity? Create(int revisionIdValue) => create?.Invoke(revisionIdValue);

    /// <summary>A predicate matching the row for <paramref name="revisionIdValue"/>.</summary>
    public Expression<Func<TEntity, bool>> ForRevision(int revisionIdValue)
    {
        // Close over a variable rather than embedding a constant, so EF parameterises the query.
        Expression<Func<int>> value = () => revisionIdValue;
        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Equal(revisionId.Body, value.Body),
            revisionId.Parameters
        );
    }
}
