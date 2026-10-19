namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>Text must be at most <see cref="Max"/> UTF-16 code units after trimming.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormMaxLengthAttribute(int max, string message) : Attribute
{
    public int Max { get; } = max;
    public string Message { get; } = message;
}
