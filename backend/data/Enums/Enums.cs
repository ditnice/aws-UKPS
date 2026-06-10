namespace UKPS.Data.Enums;

public enum WorkflowStatus
{
    Draft,
    InReview,
    Published,
    Rejected,
    Superseded
}

public enum RecordType
{
    Medicine,
    Vaccine
}

public enum QaOutcome
{
    Approved,
    Rejected
}

public enum IssueType
{
    MissingDetail,
    InvalidValue,
    InconsistentAnswer,
    Other
}

public enum ResolutionStatus
{
    Unresolved,
    Resolved
}

public enum RecordEventType
{
    RecordCreated,
    RecordStatusChanged,
    DraftCreated,
    DraftSaved,
    DraftSubmitted,
    DraftSuperseded,
    QaReviewCreated,
    QaReviewCompleted,
    QaIssueAdded,
    QaIssueResolved,
    QaIssueReopened,
    RevisionRejected,
    RevisionPublished,
    RecordReviewedNoChange
}

public enum RecordStatus
{
    Unpublished,
    Active,
    OnHold,
    Archived
}

public enum RecordStatusChangeReason
{
    DevelopmentDiscontinued,
    TrialSuspended,
    FilingWithdrawn,
    AwaitingExternalClarification,
    Other
}

public enum UserType
{
    PharmaUser,
    HorizonScanner,
    StrategicUser,
    QaUser,
    ItAdmin
}

public enum UserRole
{
    Standard,
    Champion,
    Super
}

public enum OrganisationType
{
    PharmaCompany,
    HorizonScanning,
    Strategic,
    Internal
}

/// <summary>
/// Flags enum stored as integer. Only Organisation uses Both;
/// UserOrgMembership and TermsAcceptance use Medicines or Vaccines only.
/// </summary>
[Flags]
public enum PharmaceuticalEntity
{
    Medicines = 1,
    Vaccines  = 2,
    Both      = Medicines | Vaccines
}

public enum UserOrgStatus
{
    Pending,
    Approved,
    Rejected,
    Inactive
}

public enum IamEventType
{
    FieldUpdated,
    StatusChanged,
    RoleChanged,
    Disabled,
    Enabled,
    Created,
    InformationRequested,
    TermsRequested,
    TermsSigned,
    TermsExpired
}

public enum TermsAcceptanceStatus
{
    Pending,
    Signed,
    Expired
}

public enum ReferenceDataType
{
    MedicinesOnly,
    VaccinesOnly,
    Shared
}

public enum YesNoUnknown
{
    Yes,
    No,
    Unknown
}

public enum YesNoNotYetConfirmed
{
    Yes,
    No,
    NotYetConfirmed
}

public enum DatePrecision
{
    EstimatedQuarter,
    EstimatedMonth,
    ActualDate
}

public enum DateEventType
{
    UkSubmission,
    UkLicence,
    UkLaunch,
    IntlSubmission,
    IntlLicence,
    GlobalFirstSubmission,
    EamsSubmission,
    EamsOpinion,
    EuOrphanGranted
}

public enum SubstanceNameType
{
    GenericName,
    DevelopmentName
}

public enum IndicationPaediatricStatus
{
    ExclusivelyChildren,
    BothChildrenAndAdults,
    ExclusivelyAdults,
    Unknown
}

public enum PimDesignationStatus
{
    Granted,
    NotGranted,
    DecisionPending
}

public enum EamsOpinionDecision
{
    Positive,
    Negative
}

public enum OrphanAtmpStatus
{
    Granted,
    No,
    DecisionToSubmitOngoing,
    ApplicationSubmitted
}
