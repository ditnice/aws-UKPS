using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// The ordered child rows behind a collection navigation, for the group and text list bindings.
/// Rows are ordered by <c>DisplayOrder</c> when the child has one, then by ID, and writes match
/// submitted items to stored rows by position, so a row keeps its ID while it keeps its place.
/// </summary>
internal sealed class ChildCollection
{
    private readonly PropertyInfo _navigation;
    private readonly PropertyInfo _idProperty;
    private readonly PropertyInfo? _displayOrderProperty;
    private readonly MethodInfo _add;
    private readonly MethodInfo _remove;

    public Type ChildType { get; }

    public string Description =>
        $"{_navigation.DeclaringType!.Name}.{_navigation.Name}:{ChildType.Name}"
        + (_displayOrderProperty is null ? string.Empty : $" by {_displayOrderProperty.Name}");

    public ChildCollection(
        PropertyInfo navigation,
        Type childType,
        PropertyInfo idProperty,
        PropertyInfo? displayOrderProperty
    )
    {
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
                .OrderBy(item => (int?)_displayOrderProperty?.GetValue(item) ?? int.MaxValue)
                .ThenBy(item => (int)_idProperty.GetValue(item)!)
                .ToArray();

    /// <summary>
    /// Brings the row's children in line with <paramref name="count"/> submitted items: the first
    /// stored rows are kept, extra ones are removed (after <paramref name="clear"/> has emptied
    /// their own children) and missing ones are added. Returns the rows in submitted order, with
    /// their display order set.
    /// </summary>
    public IReadOnlyList<object> Resize(
        AppDbContext dbContext,
        object row,
        int count,
        Action<object> clear
    )
    {
        IReadOnlyList<object> existing = Items(row);
        object collection = _navigation.GetValue(row) ?? CreateCollection(row);

        foreach (object extra in existing.Skip(count))
        {
            clear(extra);
            _remove.Invoke(collection, [extra]);
            dbContext.Remove(extra);
        }

        List<object> result = [.. existing.Take(count)];
        while (result.Count < count)
        {
            object item = Activator.CreateInstance(ChildType)!;
            _add.Invoke(collection, [item]);
            result.Add(item);
        }

        for (int i = 0; i < result.Count; i++)
        {
            // Stored from 1, as the synthetic data and the names and identifiers do.
            _displayOrderProperty?.SetValue(result[i], i + 1);
        }

        return result;
    }

    public Expression HasAnyExpression(ParameterExpression row) =>
        Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Any),
            [ChildType],
            Expression.Property(row, _navigation)
        );

    private object CreateCollection(object row)
    {
        object collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(ChildType))!;
        _navigation.SetValue(row, collection);
        return collection;
    }
}
