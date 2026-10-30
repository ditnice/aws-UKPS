using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>Reads or writes one property of the content row.</summary>
internal sealed class ColumnBinding(PropertyInfo property, ColumnValueKind kind) : IFormBinding
{
    private readonly Type _valueType =
        Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

    public AnswerShape Shape => AnswerShape.Single;

    public ColumnValueKind Kind => kind;

    public string Description => $"column:{property.DeclaringType!.Name}.{property.Name}:{kind}";

    public FormAnswer Read(object? row)
    {
        object? value = row is null ? null : property.GetValue(row);
        return FormAnswer.Single(
            value switch
            {
                null => null,
                string text => text,
                Enum member => member.ToString(),
                int id => id.ToString(CultureInfo.InvariantCulture),
                _ => throw new InvalidOperationException(
                    $"Unsupported column value type {value.GetType().Name}."
                ),
            }
        );
    }

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        string? value = answer.Value;
        object? converted = value is null
            ? null
            : kind switch
            {
                ColumnValueKind.Text => value,
                ColumnValueKind.Enum => Enum.Parse(_valueType, value, ignoreCase: false),
                ColumnValueKind.ReferenceId => int.Parse(value, CultureInfo.InvariantCulture),
                _ => throw new InvalidOperationException($"Unsupported column kind {kind}."),
            };
        property.SetValue(row, converted);
    }

    public Expression HasAnswerExpression(ParameterExpression row)
    {
        Expression value = Expression.Property(row, property);
        if (_valueType == typeof(string))
        {
            Expression trimmed = Expression.Call(value, nameof(string.Trim), Type.EmptyTypes);
            return Expression.AndAlso(
                Expression.NotEqual(value, Expression.Constant(null, typeof(string))),
                Expression.NotEqual(trimmed, Expression.Constant(string.Empty))
            );
        }

        if (Nullable.GetUnderlyingType(property.PropertyType) is not null)
        {
            return Expression.Property(value, nameof(Nullable<int>.HasValue));
        }

        return Expression.Constant(true);
    }
}
