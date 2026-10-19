namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>Marks a mapped property as deliberately not a question.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormIgnoreAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
