namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Progress of an application for a regulatory designation.
/// Shared by EU orphan status, EU ATMP classification and MHRA PIM designation —
/// the three questions offer the same option set, except that the ATMP question
/// does not offer <see cref="NoSubmissionIntended"/>.
/// </summary>
public enum DesignationStatus
{
    /// <summary>The designation has been granted.</summary>
    Granted = 0,

    /// <summary>An application was made and the designation was not granted.</summary>
    NotGranted = 1,

    /// <summary>A decision on whether to apply has not yet been made.</summary>
    DecisionToSubmitOngoing = 2,

    /// <summary>An application has been submitted and a decision is pending.</summary>
    ApplicationSubmittedDecisionPending = 3,

    /// <summary>No application has been made and none is intended.</summary>
    NoSubmissionIntended = 4,
}
