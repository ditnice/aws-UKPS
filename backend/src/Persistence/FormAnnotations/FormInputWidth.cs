using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The width of a text input, set with <see cref="FormFieldAttribute.Width"/>. Matches the
/// GOV.UK Design System text input width scale. Part of the HTTP API contract: the member names
/// are sent to the frontend, so never rename or remove one.
/// </summary>
/// <remarks>
/// A share of the container (<see cref="Full"/> to <see cref="OneQuarter"/>) resizes with it; a
/// character count (<see cref="Chars2"/> to <see cref="Chars30"/>) caps the input at about that
/// many characters. A <see cref="FormControl.TextList"/> takes only a share of the container.
/// </remarks>
[SuppressMessage(
    "Design",
    "CA1027:Mark enums with FlagsAttribute",
    Justification = "Each value is one width, not a combination."
)]
public enum FormInputWidth
{
    /// <summary>
    /// No override: the control's own default, two-thirds of the container. An attribute property
    /// cannot be nullable, so this stands in for null and is never sent to the frontend.
    /// </summary>
    Default = 0,

    /// <summary>The whole container.</summary>
    Full = 1,

    /// <summary>Three-quarters of the container.</summary>
    ThreeQuarters = 2,

    /// <summary>Two-thirds of the container.</summary>
    TwoThirds = 3,

    /// <summary>Half of the container.</summary>
    OneHalf = 4,

    /// <summary>A third of the container.</summary>
    OneThird = 5,

    /// <summary>A quarter of the container.</summary>
    OneQuarter = 6,

    /// <summary>About 2 characters.</summary>
    Chars2 = 7,

    /// <summary>About 3 characters.</summary>
    Chars3 = 8,

    /// <summary>About 4 characters.</summary>
    Chars4 = 9,

    /// <summary>About 5 characters.</summary>
    Chars5 = 10,

    /// <summary>About 10 characters.</summary>
    Chars10 = 11,

    /// <summary>About 20 characters.</summary>
    Chars20 = 12,

    /// <summary>About 30 characters.</summary>
    Chars30 = 13,
}
