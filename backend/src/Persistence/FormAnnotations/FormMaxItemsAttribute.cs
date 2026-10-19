namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>At most <see cref="Max"/> values may be chosen.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormMaxItemsAttribute(int max, string message) : Attribute
{
    public int Max { get; } = max;
    public string Message { get; } = message;
}
