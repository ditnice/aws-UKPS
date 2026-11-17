using System.Linq.Expressions;
using System.Text.Json.Nodes;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Stores a group answer as child rows, one per item, each item's questions written through their
/// own bindings on the child row. Items are matched to rows by position.
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
        IReadOnlyList<object> items = children.Resize(
            dbContext,
            row,
            answer.Items.Count,
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
