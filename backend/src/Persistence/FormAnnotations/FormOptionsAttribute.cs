namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Names the option provider a choice question takes its options from, when it should not use
/// the default. Without it, a choice question uses the only provider registered for its enum or
/// referenced entity (a tree provider for a <see cref="FormControl.HierarchicalSelect"/>), and
/// startup fails if there is none or more than one.
/// </summary>
/// <param name="key">
/// The provider's key, one of the <see cref="OptionKeys"/> constants. Text controls must not have
/// one. The provider must suit the property: an enum provider of the property's enum, or a
/// reference provider of the referenced entity, which must be a tree provider for a
/// <see cref="FormControl.HierarchicalSelect"/>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormOptionsAttribute(string key) : Attribute
{
    /// <summary>The option provider's key, one of the <see cref="OptionKeys"/> constants.</summary>
    public string Key { get; } = key;
}
