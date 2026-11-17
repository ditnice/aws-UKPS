using System.Linq.Expressions;
using System.Text.Json.Nodes;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Stores a group answer as child rows, one per item, each item's questions written through their
/// own bindings on the child row. With an item key, an item keeps the row that holds the same key
/// answer, ignoring case, so removing an item does not move the others onto different rows; items
/// left unmatched (new, or with an edited key) take the remaining rows in order. Without an item
/// key, items are matched to rows by position.
/// </summary>
internal sealed class GroupBinding(
    ChildCollection children,
    IReadOnlyList<FormQuestion> questions,
    string? itemKey
) : IFormBinding
{
    public AnswerShape Shape => AnswerShape.Group;

    public string Description =>
        $"group:{children.Description}[{string.Join(", ", questions.Select(static q => q.Binding.Description))}]";

    public FormAnswer Read(object? row) =>
        FormAnswer.Group(
            children
                .Items(row)
                .Select(item => new FormGroupItem(
                    questions.ToDictionary(
                        static q => q.Id,
                        q => q.Binding.Read(item),
                        StringComparer.Ordinal
                    )
                ))
        );

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        IReadOnlyList<object> items = children.Apply(
            dbContext,
            row,
            MatchRows(row, answer),
            item => Clear(dbContext, item)
        );
        for (int i = 0; i < items.Count; i++)
        {
            foreach (FormQuestion question in questions)
            {
                question.Binding.Write(dbContext, items[i], answer.Items[i][question.Id]);
            }
        }
    }

    public Expression HasAnswerExpression(ParameterExpression row) =>
        children.HasAnyExpression(row);

    /// <summary>
    /// With an item key, one JSON object keyed by each item's key answer, holding the item's
    /// other answers, so a diff can match items whatever their position or row ID. Otherwise the
    /// items in order.
    /// </summary>
    public string? AuditText(FormAnswer answer)
    {
        if (answer.IsEmpty || itemKey is null)
        {
            return answer.ToAuditText();
        }

        JsonObject audit = [];
        foreach (FormGroupItem item in answer.Items)
        {
            JsonObject rest = item.ToJsonNode();
            rest.Remove(itemKey);
            audit[item[itemKey].Value!] = rest;
        }

        return FieldAuditJson.Serialize(audit);
    }

    /// <summary>
    /// The stored row each submitted item keeps, or null for a new row. First each item takes the
    /// row with the same item key answer. Then each item still unmatched takes the first unclaimed
    /// row between its neighbours' rows, so the stored order is kept even for a child read back in
    /// ID order; with no such row it gets a new one.
    /// </summary>
    private object?[] MatchRows(object row, FormAnswer answer)
    {
        IReadOnlyList<object> existing = children.Items(row);
        int[] positions = new int[answer.Items.Count];
        Array.Fill(positions, -1);
        bool[] claimed = new bool[existing.Count];

        MatchByKey(existing, answer, positions, claimed);
        FillInOrder(positions, claimed);

        return [.. positions.Select(p => p < 0 ? null : existing[p])];
    }

    /// <summary>Gives each item with an item key answer the first unclaimed row holding it.</summary>
    private void MatchByKey(
        IReadOnlyList<object> existing,
        FormAnswer answer,
        int[] positions,
        bool[] claimed
    )
    {
        if (itemKey is null)
        {
            return;
        }

        IFormBinding keyBinding = questions
            .Single(q => string.Equals(q.Id, itemKey, StringComparison.Ordinal))
            .Binding;
        string?[] storedKeys = [.. existing.Select(e => keyBinding.Read(e).Value)];
        for (int i = 0; i < positions.Length; i++)
        {
            if (answer.Items[i][itemKey].Value is not { } key)
            {
                continue;
            }

            int match = Enumerable
                .Range(0, storedKeys.Length)
                .FirstOrDefault(
                    e =>
                        !claimed[e]
                        && string.Equals(storedKeys[e], key, StringComparison.OrdinalIgnoreCase),
                    -1
                );
            if (match >= 0)
            {
                positions[i] = match;
                claimed[match] = true;
            }
        }
    }

    /// <summary>
    /// Gives each unmatched item the first unclaimed row after the previous item's row and before
    /// the next matched item's row, when there is one.
    /// </summary>
    private static void FillInOrder(int[] positions, bool[] claimed)
    {
        int previous = -1;
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i] < 0)
            {
                int following = positions.Skip(i + 1).FirstOrDefault(static p => p >= 0, -1);
                if (following < 0)
                {
                    following = claimed.Length;
                }

                int free = Array.FindIndex(claimed, previous + 1, static c => !c);
                if (free >= 0 && free < following)
                {
                    positions[i] = free;
                    claimed[free] = true;
                }
            }

            previous = Math.Max(previous, positions[i]);
        }
    }

    /// <summary>Removes an item's own child rows before the item is deleted.</summary>
    private void Clear(AppDbContext dbContext, object item)
    {
        foreach (
            FormQuestion question in questions.Where(static q => q.Shape != AnswerShape.Single)
        )
        {
            question.Binding.Write(dbContext, item, FormAnswer.Empty(question.Shape));
        }
    }
}
