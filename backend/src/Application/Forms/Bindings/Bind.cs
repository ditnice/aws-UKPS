using System.Linq.Expressions;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Factory methods for bindings, built from typed expressions so entity renames break the
/// build.
/// </summary>
internal static class Bind
{
    /// <summary>Binds to a property on a revision's content row, e.g.
    /// <c>Bind.Column((MedicinesProductDetail x) => x.Indication)</c>.</summary>
    public static IQuestionBinding Column<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> property
    )
        where TEntity : class =>
        new ColumnBinding<TEntity, TValue>(ContentRows.Get<TEntity>(), property);

    /// <summary>Binds to a junction table under a content row.</summary>
    public static IQuestionBinding Junction<TParent, TJunction>(
        Expression<Func<TJunction, int>> parentId,
        Expression<Func<TJunction, int>> referenceId
    )
        where TParent : class
        where TJunction : class, new() =>
        new JunctionBinding<TParent, TJunction>(ContentRows.Get<TParent>(), parentId, referenceId);
}
