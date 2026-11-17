using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Stores an ordered list of strings as child rows with one string column each, such as a
/// product's generic names.
/// </summary>
internal sealed class TextListBinding(ChildCollection children, PropertyInfo valueProperty)
    : IFormBinding
{
    public AnswerShape Shape => AnswerShape.List;

    public string Description => $"text-list:{children.Description} value {valueProperty.Name}";

    public FormAnswer Read(object? row) =>
        FormAnswer.List(children.Items(row).Select(item => (string)valueProperty.GetValue(item)!));

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        IReadOnlyList<object> items = children.Resize(
            dbContext,
            row,
            answer.Values.Count,
            static _ => { }
        );
        for (int i = 0; i < items.Count; i++)
        {
            valueProperty.SetValue(items[i], answer.Values[i]);
        }
    }

    public Expression HasAnswerExpression(ParameterExpression row) =>
        children.HasAnyExpression(row);
}
