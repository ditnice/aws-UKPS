namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>The question must be answered.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormRequiredAttribute(string message) : Attribute
{
    public string Message { get; } = message;
}
