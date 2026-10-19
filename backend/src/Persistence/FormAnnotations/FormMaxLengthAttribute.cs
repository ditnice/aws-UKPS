namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>Text must be at most <see cref="Max"/> UTF-16 code units after trimming.</summary>
/// <param name="max">
/// The most UTF-16 code units allowed after trimming. Must be positive. Only a
/// <see cref="FormControl.Text"/> or <see cref="FormControl.Textarea"/> question may have it.
/// </param>
/// <param name="message">
/// The error shown against the question when the text is too long, for example
/// <c>"Indication must be 2000 characters or fewer"</c>.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormMaxLengthAttribute(int max, string message) : Attribute
{
    /// <summary>The most UTF-16 code units allowed after trimming.</summary>
    public int Max { get; } = max;

    /// <summary>The error shown when the text is too long.</summary>
    public string Message { get; } = message;
}
