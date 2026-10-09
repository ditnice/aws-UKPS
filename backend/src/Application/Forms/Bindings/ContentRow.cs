using System.Linq.Expressions;

namespace UKPS.Api.Application.Forms.Bindings;

/// <inheritdoc cref="IContentRow"/>
/// <remarks>
/// Only rows created by record creation are supported so far: a missing row is an invariant
/// failure rather than something the form creates. Rows created on first write (and the
/// startup check that their non-nullable columns are bound) come with the first page that
/// needs them.
/// </remarks>
internal sealed class ContentRow<TEntity>(
    Expression<Func<TEntity, int>> revisionId,
    Expression<Func<TEntity, int>> id
) : IContentRow
    where TEntity : class
{
    private readonly Func<TEntity, int> _getId = id.Compile();

    public Type EntityType => typeof(TEntity);

    public int GetId(TEntity entity) => _getId(entity);

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
