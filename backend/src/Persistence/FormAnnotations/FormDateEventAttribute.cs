using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// How a <see cref="FormControl.PrecisionDate"/> or <see cref="FormControl.ActualMonth"/> question
/// stores its answer: a regulatory date row with this event, which the question's foreign key
/// points to. Required on those controls and not allowed on any other.
/// </summary>
/// <param name="dateEvent">The event the date is for, for example the UK submission.</param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormDateEventAttribute(DateEventType dateEvent) : Attribute
{
    /// <summary>The event the date is for.</summary>
    public DateEventType DateEvent { get; } = dateEvent;

    /// <summary>
    /// What the date is of, as its error messages name it, for example <c>"submission"</c> in
    /// "Enter the actual submission date" and "Actual date of submission must be in the past".
    /// Required on a <see cref="FormControl.PrecisionDate"/>.
    /// </summary>
    public string? Name { get; init; }
}
