using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Maps a <see cref="FlagsAttribute"/> enum property to the names of its set members, combined
/// with OR on write.
/// </summary>
internal sealed class FlagsBinding : IFormBinding
{
    private readonly PropertyInfo _property;
    private readonly Type _enumType;
    private readonly bool _isNullable;

    public FlagsBinding(PropertyInfo property)
    {
        _property = property;
        Type? underlying = Nullable.GetUnderlyingType(property.PropertyType);
        _isNullable = underlying is not null;
        _enumType = underlying ?? property.PropertyType;
    }

    public AnswerShape Shape => AnswerShape.Multi;

    public string Description => $"flags:{_property.DeclaringType!.Name}.{_property.Name}";

    public FormAnswer Read(object? row)
    {
        if (row is null || _property.GetValue(row) is not Enum value)
        {
            return FormAnswer.Multi([]);
        }

        long bits = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
        IEnumerable<string> names = Enum.GetValues(_enumType)
            .Cast<Enum>()
            .Select(member =>
                (
                    Name: member.ToString(),
                    Bits: Convert.ToInt64(member, System.Globalization.CultureInfo.InvariantCulture)
                )
            )
            .Where(member => member.Bits != 0 && (bits & member.Bits) == member.Bits)
            .Select(static member => member.Name);
        return FormAnswer.Multi(names);
    }

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        long bits = 0;
        foreach (string name in answer.Values)
        {
            bits |= Convert.ToInt64(
                Enum.Parse(_enumType, name, ignoreCase: false),
                System.Globalization.CultureInfo.InvariantCulture
            );
        }

        object? value = bits == 0 && _isNullable ? null : Enum.ToObject(_enumType, bits);
        _property.SetValue(row, value);
    }

    public Expression HasAnswerExpression(ParameterExpression row)
    {
        Expression value = Expression.Property(row, _property);
        Expression enumValue = _isNullable
            ? Expression.Property(value, nameof(Nullable<int>.Value))
            : value;
        Expression hasFlag = Expression.NotEqual(
            enumValue,
            Expression.Constant(Enum.ToObject(_enumType, 0), _enumType)
        );

        return _isNullable
            ? Expression.AndAlso(
                Expression.Property(value, nameof(Nullable<int>.HasValue)),
                hasFlag
            )
            : hasFlag;
    }
}
