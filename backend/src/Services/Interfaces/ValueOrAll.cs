using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace UKPS.Api.Services.Interfaces;

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ValueOrAll<T>
{
    private readonly ValueOrAllState _state;
    private readonly T? _value;

    private ValueOrAll(ValueOrAllState state, T? value)
    {
        _state = state;
        _value = value;
    }

    public static ValueOrAll<T> All => new(ValueOrAllState.All, default);

    public static ValueOrAll<T> FromValue(T value) => new(ValueOrAllState.Value, value);

    public static ValueOrAll<T> None() => new(ValueOrAllState.None, default);

    public bool Contains(T value) =>
        (_state, _value) switch
        {
            (ValueOrAllState.All, _) => true,
            (ValueOrAllState.None, _) => false,
            (ValueOrAllState.Value, var v) => EqualityComparer<T>.Default.Equals(v, value),
            _ => false,
        };

    /// <summary>
    /// Converts the ValueOrAll instance into a filter expression that can be used in EF Core queries.
    /// </summary>
    public Expression<Func<TEntity, bool>> Contains<TEntity>(Expression<Func<TEntity, T>> selector)
    {
        switch (_state)
        {
            case ValueOrAllState.All:
                return _ => true;

            case ValueOrAllState.None:
                return _ => false;

            case ValueOrAllState.Value:
                return Expression.Lambda<Func<TEntity, bool>>(
                    Expression.Equal(selector.Body, Expression.Constant(_value, typeof(T))),
                    selector.Parameters
                );

            default:
                throw new UnreachableException();
        }
    }

    public static implicit operator ValueOrAll<T>(T value) => FromValue(value);

    private enum ValueOrAllState
    {
        None = 0,
        Value = 1,
        All = 2,
    }
}
