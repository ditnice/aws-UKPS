namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// No entry of a <see cref="FormControl.TextList"/> may repeat another, ignoring case. The error
/// is shown against each repeat after the first.
/// </summary>
/// <param name="message">
/// The error shown against a repeated entry, for example
/// <c>"Generic name has already been entered"</c>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormDistinctAttribute(string message) : Attribute
{
    /// <summary>The error shown against a repeated entry.</summary>
    public string Message { get; } = message;
}
