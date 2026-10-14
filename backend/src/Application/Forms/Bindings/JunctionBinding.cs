using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Binds a multi-value question to a junction table linking a content row to reference data.
/// The answer is the set of selected reference IDs as strings, in ascending ID order. Writing
/// diffs against the stored rows and inserts/deletes only what changed. If the parent row does
/// not exist yet, the answer is empty and the first write creates it.
/// </summary>
internal sealed class JunctionBinding<TParent, TJunction> : IQuestionBinding
    where TParent : class
    where TJunction : class, new()
{
    private readonly ContentRow<TParent> _parentRow;
    private readonly Expression<Func<TJunction, int>> _parentId;
    private readonly Func<TJunction, int> _getReferenceId;
    private readonly PropertyInfo _parentIdProperty;
    private readonly PropertyInfo _referenceIdProperty;

    public JunctionBinding(
        ContentRow<TParent> parentRow,
        Expression<Func<TJunction, int>> parentId,
        Expression<Func<TJunction, int>> referenceId
    )
    {
        _parentRow = parentRow;
        _parentId = parentId;
        _getReferenceId = referenceId.Compile();
        _parentIdProperty = GetProperty(parentId, nameof(parentId));
        _referenceIdProperty = GetProperty(referenceId, nameof(referenceId));
    }

    public BindingValueKind ValueKind => BindingValueKind.ReferenceIdList;

    public Type? EnumType => null;

    public string Description =>
        $"Junction {typeof(TJunction).Name}({_parentIdProperty.Name}, {_referenceIdProperty.Name})";

    public async Task<JsonNode?> ReadAsync(
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var parent = await context.FindRowAsync(_parentRow, cancellationToken);
        var rows = await LoadRowsAsync(context, parent, cancellationToken);
        return AnswerValues.ToArray(
            rows.Select(_getReferenceId)
                .Order()
                .Select(id => id.ToString(CultureInfo.InvariantCulture))
        );
    }

    public async Task WriteAsync(
        BindingContext context,
        JsonNode? value,
        CancellationToken cancellationToken
    )
    {
        var parent = await context.GetOrCreateRowAsync(_parentRow, cancellationToken);
        var selected = AnswerValues
            .GetStrings(value)
            .Select(s => int.Parse(s, NumberStyles.None, CultureInfo.InvariantCulture))
            .ToHashSet();

        var existing = await LoadRowsAsync(context, parent, cancellationToken);
        var set = context.DbContext.Set<TJunction>();
        set.RemoveRange(existing.Where(row => !selected.Contains(_getReferenceId(row))));

        var existingIds = existing.Select(_getReferenceId).ToHashSet();
        var parentNavigation = GetParentNavigation(context);
        foreach (var referenceId in selected.Where(id => !existingIds.Contains(id)))
        {
            var row = new TJunction();
            _referenceIdProperty.SetValue(row, referenceId);
            set.Add(row);
            // Link through the navigation rather than the foreign key, so this also works for a
            // parent created in this save, whose ID is not known until it is inserted.
            context.DbContext.Entry(row).Navigation(parentNavigation).CurrentValue = parent;
        }
    }

    private async Task<List<TJunction>> LoadRowsAsync(
        BindingContext context,
        TParent? parent,
        CancellationToken cancellationToken
    )
    {
        if (parent is null || context.IsNew(parent))
        {
            return [];
        }

        var parentId = _parentRow.GetId(parent);
        Expression<Func<int>> parentIdValue = () => parentId;
        var predicate = Expression.Lambda<Func<TJunction, bool>>(
            Expression.Equal(_parentId.Body, parentIdValue.Body),
            _parentId.Parameters
        );

        return await context
            .DbContext.Set<TJunction>()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    private static string GetParentNavigation(BindingContext context) =>
        context
            .DbContext.Model.FindEntityType(typeof(TJunction))
            ?.GetNavigations()
            .SingleOrDefault(navigation => navigation.TargetEntityType.ClrType == typeof(TParent))
            ?.Name
        ?? throw new InvalidOperationException(
            $"{typeof(TJunction).Name} has no navigation to {typeof(TParent).Name}."
        );

    private static PropertyInfo GetProperty(
        Expression<Func<TJunction, int>> expression,
        string parameterName
    ) =>
        expression.Body
            is MemberExpression { Member: PropertyInfo { SetMethod: not null } property }
            ? property
            : throw new ArgumentException(
                "Expected a settable property of the junction entity.",
                parameterName
            );
}
