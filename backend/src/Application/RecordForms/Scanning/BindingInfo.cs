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

    /// <summary>The navigation that loads the regulatory date of a regulatory date binding.</summary>
    public PropertyInfo? DateNavigation { get; init; }

    public Type? JunctionType { get; init; }
    public PropertyInfo? JunctionReferenceIdProperty { get; init; }

    /// <summary>The child entity of a group or text list binding.</summary>
    public Type? ChildType { get; init; }

    /// <summary>The child entity's integer primary key, which orders rows after display order.</summary>
    public PropertyInfo? ChildIdProperty { get; init; }

    /// <summary>The child entity's <c>int? DisplayOrder</c>, when it has one.</summary>
    public PropertyInfo? ChildDisplayOrderProperty { get; init; }

    /// <summary>The string column holding each entry of a text list binding.</summary>
    public PropertyInfo? ChildValueProperty { get; init; }

    /// <summary>
    /// The collection navigation holding a text list's rows: the property itself, or the one its
    /// <c>[FormListSource]</c> names.
    /// </summary>
    public PropertyInfo? ChildNavigation { get; init; }

    /// <summary>The enum column that tells a shared collection's lists apart, if any.</summary>
    public PropertyInfo? ChildKindProperty { get; init; }

    /// <summary>The enum member of this list's rows in a shared collection.</summary>
    public object? ChildKind { get; init; }
}
