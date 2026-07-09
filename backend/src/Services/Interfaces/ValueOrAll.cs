using System.Runtime.InteropServices;

namespace UKPS.Api.Services.Interfaces;

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ValueOrAll<T>
{
    public bool IsAll { get; }
    public T? Value { get; }

    private ValueOrAll(bool isAll, T? value)
    {
        IsAll = isAll;
        Value = value;
    }

    public static ValueOrAll<T> All => new(true, default);

    public static ValueOrAll<T> FromValue(T value) => new(false, value);

    public static ValueOrAll<T> None() => new(false, default);

    public bool Contains(T value) => IsAll || EqualityComparer<T>.Default.Equals(Value, value);

    public static implicit operator ValueOrAll<T>(T value) => FromValue(value);
}
