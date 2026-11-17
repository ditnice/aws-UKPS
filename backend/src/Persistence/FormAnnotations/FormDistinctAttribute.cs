namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// No value may be repeated, ignoring case. On a <see cref="FormControl.TextList"/> the entries
/// of the list must differ; on a child question of a <see cref="FormControl.Group"/> its answer
/// must differ between the group's items. The error is shown against each repeat after the first.
/// </summary>
/// <param name="message">
/// The error shown against a repeated value, for example
/// <c>"Trial number has already been entered"</c>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormDistinctAttribute(string message) : Attribute
{
    /// <summary>The error shown against a repeated value.</summary>
    public string Message { get; } = message;
}
