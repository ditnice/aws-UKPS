using UKPS.Data.Enums;

namespace UKPS.Data.Entities.SharedRevisionContent;

public class RegulatoryDate
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public DateEventType DateEvent { get; set; }
    public DatePrecision DatePrecision { get; set; }

    /// <summary>Quarter/Month stored as first day of that period.</summary>
    public DateOnly DateValue { get; set; }

    /// <summary>True for all estimated precision dates.</summary>
    public bool IsConfidential { get; set; }

    /// <summary>Relevant only for uk_licence and intl_licence events.</summary>
    public bool? ConditionalApprovalAnticipated { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}

public class RecordMhraProcedure
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? MhraProcedureTypeId { get; set; }
    public string? ProcedureDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.MhraProcedureType? MhraProcedureType { get; set; }
}

public class RecordMhraDate
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? UkSubmissionDateId { get; set; }
    public int? UkLicenceDateId { get; set; }
    public int? UkLaunchDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public RegulatoryDate? UkSubmissionDate { get; set; }
    public RegulatoryDate? UkLicenceDate { get; set; }
    public RegulatoryDate? UkLaunchDate { get; set; }
}

public class RecordIntlRecognition
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? IrpReferenceRegulatorId { get; set; }
    public int? IrpRouteId { get; set; }
    public YesNoUnknown? IntlConditionalApprovalAnticipated { get; set; }
    public int? IntlSubmissionDateId { get; set; }
    public int? IntlLicenceDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.IrpReferenceRegulator? IrpReferenceRegulator { get; set; }
    public ReferenceData.IrpRoute? IrpRoute { get; set; }
    public RegulatoryDate? IntlSubmissionDate { get; set; }
    public RegulatoryDate? IntlLicenceDate { get; set; }
}

public class RecordGlobalSubmission
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? GlobalFirstSubmissionRegion { get; set; }
    public string? GlobalFirstSubmissionNotes { get; set; }
    public int? GlobalSubmissionEstimatedDateId { get; set; }
    public int? GlobalSubmissionActualDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public RegulatoryDate? GlobalSubmissionEstimatedDate { get; set; }
    public RegulatoryDate? GlobalSubmissionActualDate { get; set; }
}

public class RecordHta
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Vaccines only: JCVI, NICE, or Not applicable (single-select radio).</summary>
    public string? HtaBodyVaccine { get; set; }

    /// <summary>Medicines only. Conditional on NICE being selected.</summary>
    public YesNoUnknown? HtaNiceAlignedPathway { get; set; }

    public string? HtaAdditionalDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<RecordHtaBody> HtaBodies { get; set; } = [];
}

/// <summary>
/// HTA bodies selected for a medicine record.
/// Stored as label varchar (NICE, SMC, AWMSG) rather than FK to organisation —
/// HTA bodies are a small, stable set that do not require the full organisation model.
/// </summary>
public class RecordHtaBody
{
    public int RecordHtaId { get; set; }

    /// <summary>e.g. NICE, SMC, AWMSG</summary>
    public string Label { get; set; } = null!;

    // Navigation
    public RecordHta RecordHta { get; set; } = null!;
}

public class RecordClinicalTrial
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string StudyName { get; set; } = null!;

    /// <summary>8-digit NCT identifier; system auto-links to ClinicalTrials.gov.</summary>
    public string? ClinicalTrialsGovNumber { get; set; }

    public string? BriefDescription { get; set; }
    public YesNoUnknown? RecruitingInUk { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<OtherClinicalTrialNumber> OtherClinicalTrialNumbers { get; set; } = [];
}

public class OtherClinicalTrialNumber
{
    public int Id { get; set; }
    public int ClinicalTrialId { get; set; }

    /// <summary>e.g. ISRCTN, EudraCT</summary>
    public string OtherRegistryNumber { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    // Navigation
    public RecordClinicalTrial ClinicalTrial { get; set; } = null!;
}
