using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// The ordered child rows behind a collection navigation, for the group and text list bindings.
/// Rows are ordered by <c>DisplayOrder</c> when the child has one, then by ID. The caller decides
/// which stored row each submitted item keeps (<see cref="Apply"/>); <see cref="Resize"/> matches
/// them by position.
/// </summary>
/// <remarks>
/// When the collection is shared between several lists, a kind filter limits it to the rows whose
/// enum column holds one member; other rows are never read, moved or removed.
/// </remarks>
internal sealed class ChildCollection
{
    private readonly PropertyInfo _navigation;
    private readonly PropertyInfo _idProperty;
    private readonly PropertyInfo? _displayOrderProperty;
    private readonly PropertyInfo? _kindProperty;
    private readonly object? _kind;
    private readonly MethodInfo _add;
    private readonly MethodInfo _remove;

    public Type ChildType { get; }

    public string Description =>
        $"{_navigation.DeclaringType!.Name}.{_navigation.Name}:{ChildType.Name}"
        + (_displayOrderProperty is null ? string.Empty : $" by {_displayOrderProperty.Name}")
        + (_kindProperty is null ? string.Empty : $" where {_kindProperty.Name} = {_kind}");

    /// <summary>
    /// The children behind <paramref name="navigation"/>, limited to those whose
    /// <paramref name="kindProperty"/> holds <paramref name="kind"/> when one is given.
    /// </summary>
    public ChildCollection(
        PropertyInfo navigation,
        Type childType,
        PropertyInfo idProperty,
        PropertyInfo? displayOrderProperty,
        PropertyInfo? kindProperty = null,
        object? kind = null
    )
    {
        _kindProperty = kindProperty;
        _kind = kind;
        _navigation = navigation;
        _idProperty = idProperty;
        _displayOrderProperty = displayOrderProperty;
        ChildType = childType;
        Type collectionType = typeof(ICollection<>).MakeGenericType(childType);
        _add = collectionType.GetMethod(nameof(ICollection<>.Add))!;
        _remove = collectionType.GetMethod(nameof(ICollection<>.Remove))!;
    }

    /// <summary>The row's children in display order; none when there is no row.</summary>
    public IReadOnlyList<object> Items(object? row) =>
        row is null || _navigation.GetValue(row) is not IEnumerable items
            ? []
            : items
                .Cast<object>()
                .Where(IsOfKind)
                .OrderBy(item => (int?)_displayOrderProperty?.GetValue(item) ?? int.MaxValue)
                .ThenBy(item => (int)_idProperty.GetValue(item)!)
                .ToArray();

    /// <summary>
    /// Brings the row's children in line with <paramref name="count"/> submitted items, matched by
    /// position: the first stored rows are kept, extra ones are removed and missing ones are added.
    /// </summary>
    public IReadOnlyList<object> Resize(
        AppDbContext dbContext,
        object row,
        int count,
        Action<object> clear
    )
    {
        IReadOnlyList<object> existing = Items(row);
        return Apply(
            dbContext,
            row,
            [.. Enumerable.Range(0, count).Select(i => i < existing.Count ? existing[i] : null)],
            clear
        );
    }

    /// <summary>
    /// Brings the row's children in line with the submitted items. <paramref name="kept"/> holds,
    /// for each submitted item in order, the stored child row it keeps, or null for a new row.
    /// Stored rows that are not kept are removed, after <paramref name="clear"/> has emptied their
    /// own children. Returns the rows in submitted order, with their display order (and kind) set.
    /// </summary>
    /// <remarks>
    /// A child with no <c>DisplayOrder</c> is read back in ID order, so keeping rows out of their
    /// stored order would reorder the items.
    /// </remarks>
    public IReadOnlyList<object> Apply(
        AppDbContext dbContext,
        object row,
        IReadOnlyList<object?> kept,
        Action<object> clear
    )
    {
        IReadOnlyList<object> existing = Items(row);
        object collection = _navigation.GetValue(row) ?? CreateCollection(row);
        HashSet<object> keptRows = new(kept.OfType<object>(), ReferenceEqualityComparer.Instance);

        foreach (object extra in existing.Where(e => !keptRows.Contains(e)))
        {
            clear(extra);
            _remove.Invoke(collection, [extra]);
            dbContext.Remove(extra);
        }

        List<object> result = new(kept.Count);
        foreach (object? keptRow in kept)
        {
            if (keptRow is not null)
            {
                result.Add(keptRow);
                continue;
            }

            object item = Activator.CreateInstance(ChildType)!;
            _kindProperty?.SetValue(item, _kind);
            _add.Invoke(collection, [item]);
            result.Add(item);
        }

        for (int i = 0; i < result.Count; i++)
        {
            // Stored from 1, as record creation and the synthetic data do.
            _displayOrderProperty?.SetValue(result[i], i + 1);
        }

        return result;
    }

    public Expression HasAnyExpression(ParameterExpression row) =>
        _kindProperty is null
            ? Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                [ChildType],
                Expression.Property(row, _navigation)
            )
            : Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                [ChildType],
                Expression.Property(row, _navigation),
                IsOfKindExpression()
            );

    private LambdaExpression IsOfKindExpression()
    {
        ParameterExpression child = Expression.Parameter(ChildType, "child");
        return Expression.Lambda(
            Expression.Equal(
                Expression.Property(child, _kindProperty!),
                Expression.Constant(_kind, _kindProperty!.PropertyType)
            ),
            child
        );
    }

    private bool IsOfKind(object item) =>
        _kindProperty is null || Equals(_kindProperty.GetValue(item), _kind);

    private object CreateCollection(object row)
    {
        object collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(ChildType))!;
        _navigation.SetValue(row, collection);
        return collection;
    }
}
