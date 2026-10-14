using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>The content rows that form questions may bind to.</summary>
internal static class ContentRows
{
    // Record creation does not insert this row, so the first save that writes to it does.
    // Every column is nullable, so a new row needs only its revision.
    public static ContentRow<MedicinesIndicationDetail> MedicinesIndicationDetail { get; } =
        new(
            x => x.RevisionId,
            x => x.Id,
            revisionId => new MedicinesIndicationDetail { RevisionId = revisionId }
        );

    private static readonly Dictionary<Type, IContentRow> _rows = new[]
    {
        (IContentRow)MedicinesIndicationDetail,
    }.ToDictionary(row => row.EntityType);

    public static ContentRow<TEntity> Get<TEntity>()
        where TEntity : class =>
        _rows.TryGetValue(typeof(TEntity), out var row)
            ? (ContentRow<TEntity>)row
            : throw new InvalidOperationException(
                $"{typeof(TEntity).Name} is not registered in {nameof(ContentRows)}."
            );
}
