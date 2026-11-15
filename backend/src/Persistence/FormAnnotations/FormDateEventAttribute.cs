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
}
