namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>Names the option provider a choice question takes its options from.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormOptionsAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
