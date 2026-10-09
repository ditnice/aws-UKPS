using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Binds a question to one property of a revision's content row. Supports string, enum
/// (answer = member name) and integer reference ID (answer = ID as a string) properties,
/// nullable or not.
/// </summary>
internal sealed class ColumnBinding<TEntity, TValue> : IQuestionBinding
    where TEntity : class
{
    private readonly ContentRow<TEntity> _row;
    private readonly string _propertyName;
    private readonly Func<TEntity, TValue> _get;
    private readonly Action<TEntity, TValue> _set;
    private readonly Func<TValue, JsonNode?> _toAnswer;
    private readonly Func<JsonNode?, TValue> _fromAnswer;

    public ColumnBinding(ContentRow<TEntity> row, Expression<Func<TEntity, TValue>> property)
    {
        if (
            property.Body is not MemberExpression { Member: PropertyInfo propertyInfo }
            || propertyInfo.SetMethod is null
        )
        {
            throw new ArgumentException(
                "The binding must be a settable property of the entity, e.g. x => x.Indication.",
                nameof(property)
            );
        }

        _row = row;
        _propertyName = propertyInfo.Name;
        _get = property.Compile();
        var value = Expression.Parameter(typeof(TValue), "value");
        _set = Expression
            .Lambda<Action<TEntity, TValue>>(
                Expression.Assign(property.Body, value),
                property.Parameters[0],
                value
            )
            .Compile();

        (ValueKind, EnumType, _toAnswer, _fromAnswer) = CreateConverters();
    }

    public BindingValueKind ValueKind { get; }

    public Type? EnumType { get; }

    public string Description => $"Column {typeof(TEntity).Name}.{_propertyName}";

    public async Task<JsonNode?> ReadAsync(
        BindingContext context,
        CancellationToken cancellationToken
    )
    {
        var entity = await context.GetRowAsync(_row, cancellationToken);
        return _toAnswer(_get(entity));
    }

    public async Task WriteAsync(
        BindingContext context,
        JsonNode? value,
        CancellationToken cancellationToken
    )
    {
        var entity = await context.GetRowAsync(_row, cancellationToken);
        _set(entity, _fromAnswer(value));
    }

    private static (
        BindingValueKind Kind,
        Type? EnumType,
        Func<TValue, JsonNode?> ToAnswer,
        Func<JsonNode?, TValue> FromAnswer
    ) CreateConverters()
    {
        var valueType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        if (valueType == typeof(string))
        {
            return (
                BindingValueKind.Text,
                null,
                v => v is null ? null : JsonValue.Create((string)(object)v),
                a => AnswerValues.TryGetString(a, out var s) ? (TValue)(object)s : default!
            );
        }

        if (valueType.IsEnum)
        {
            return (
                BindingValueKind.Enum,
                valueType,
                v => v is null ? null : JsonValue.Create(v.ToString()),
                a =>
                    AnswerValues.TryGetString(a, out var s)
                        ? (TValue)Enum.Parse(valueType, s, ignoreCase: false)
                        : default!
            );
        }

        if (valueType == typeof(int))
        {
            return (
                BindingValueKind.ReferenceId,
                null,
                v =>
                    v is null
                        ? null
                        : JsonValue.Create(((int)(object)v).ToString(CultureInfo.InvariantCulture)),
                a =>
                    AnswerValues.TryGetString(a, out var s)
                        ? (TValue)
                            (object)int.Parse(s, NumberStyles.None, CultureInfo.InvariantCulture)
                        : default!
            );
        }

        throw new NotSupportedException(
            $"Column bindings do not support {typeof(TValue).Name} properties."
        );
    }
}
