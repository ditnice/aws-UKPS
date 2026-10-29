using System.Reflection;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>What the EF model says about where a question's answer is stored.</summary>
internal sealed record BindingInfo(BindingKind Kind)
{
    /// <summary>False when the bound column cannot hold null.</summary>
    public bool IsNullable { get; init; } = true;

    /// <summary>The enum type of an enum or flags binding.</summary>
    public Type? EnumType { get; init; }

    /// <summary>The reference data entity of a reference ID or junction binding.</summary>
    public Type? ReferenceType { get; init; }

    public Type? JunctionType { get; init; }
    public PropertyInfo? JunctionReferenceIdProperty { get; init; }
}
