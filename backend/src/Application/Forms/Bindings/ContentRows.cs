using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>The content rows that form questions may bind to.</summary>
internal static class ContentRows
{
    public static ContentRow<MedicinesProductDetail> MedicinesProductDetail { get; } =
        new(x => x.RevisionId, x => x.Id);

    private static readonly Dictionary<Type, IContentRow> _rows = new[]
    {
        (IContentRow)MedicinesProductDetail,
    }.ToDictionary(row => row.EntityType);

    public static ContentRow<TEntity> Get<TEntity>()
        where TEntity : class =>
        _rows.TryGetValue(typeof(TEntity), out var row)
            ? (ContentRow<TEntity>)row
            : throw new InvalidOperationException(
                $"{typeof(TEntity).Name} is not registered in {nameof(ContentRows)}."
            );
}
