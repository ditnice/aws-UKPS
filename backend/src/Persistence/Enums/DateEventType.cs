namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the different types of date-related events.
/// </summary>
public enum DateEventType
{
    /// <summary>
    /// Represents the UK submission event.
    /// </summary>
    UkSubmission = 0,

    /// <summary>
    /// Represents the UK licence event.
    /// </summary>
    UkLicence = 1,

    /// <summary>
    /// Represents the UK launch event.
    /// </summary>
    UkLaunch = 2,

    /// <summary>
    /// Represents the international submission event.
    /// </summary>
    IntlSubmission = 3,

    /// <summary>
    /// Represents the international licence event.
    /// </summary>
    IntlLicence = 4,

    /// <summary>
    /// Represents the global first submission event.
    /// </summary>
    GlobalFirstSubmission = 5,

    /// <summary>
    /// Represents the EAMS (Early Access to Medicines Scheme) submission event.
    /// </summary>
    EamsSubmission = 6,

    /// <summary>
    /// Represents the EAMS opinion event.
    /// </summary>
    EamsOpinion = 7,

    /// <summary>
    /// Represents the EU orphan designation granted event.
    /// </summary>
    EuOrphanGranted = 8,
}
