namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The question must be answered. Required on every question bound to a non-nullable column.
/// </summary>
/// <param name="message">
/// The error shown against the question when it is left blank, for example
/// <c>"Enter the indication"</c>. The server and the browser show the same text.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormRequiredAttribute(string message) : Attribute
{
    /// <summary>The error shown when the question is left blank.</summary>
    public string Message { get; } = message;
}
