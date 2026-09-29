using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Health technology assessment and UK launch.</summary>
internal sealed class RecordHta
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Vaccines only: JCVI, NICE, or Not applicable (single-select radio).</summary>
    public VaccineHtaAssessor? VaccineHtaAssessor { get; set; }

    /// <summary>
    /// Medicines only: is an HTA submission planned for this indication?
    /// </summary>
    public YesNoUnknown? MedicineHtaSubmissionIntended { get; set; }

    /// <summary>
    /// Medicines only, multi-select.
    /// </summary>
    public MedicineHtaAssessor? MedicineHtaBodies { get; set; }

    public string? HtaAdditionalDetails { get; set; }

    /// <summary>Medicines only. Conditional on NICE being selected.</summary>
    public YesNoUnknown? HtaNiceAlignedPathway { get; set; }

    /// <summary>
    /// Medicines only. The unique identifier assigned by NICE to this technology appraisal.
    /// Format: GID-TAXXXX or GID-HSTXXXX. Optional.
    /// </summary>
    public string? NiceTaDevelopmentId { get; set; }

    public int? UkLaunchDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public RegulatoryDate? UkLaunchDate { get; set; }
}
