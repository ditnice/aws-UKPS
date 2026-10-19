namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Marks a mapped property as deliberately not a question. On a form section entity, every
/// mapped column needs either this or <see cref="FormFieldAttribute"/>; navigations need neither.
/// </summary>
/// <param name="reason">
/// Why the column is not a question, for example <c>"No page designed yet"</c>. Must not be
/// blank. The property must not also have <see cref="FormFieldAttribute"/> or a rule attribute.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormIgnoreAttribute(string reason) : Attribute
{
    /// <summary>Why the column is not a question.</summary>
    public string Reason { get; } = reason;
}
