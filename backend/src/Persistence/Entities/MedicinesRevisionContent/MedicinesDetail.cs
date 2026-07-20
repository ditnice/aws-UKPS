using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string? ModeOfAction { get; set; }
    public string? ProposedDoseRegimen { get; set; }
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }
    public YesNoUnknown? IsRepurposedMedicine { get; set; }

    /// <summary>Conditional on IsRepurposedMedicine = Yes.</summary>
    public string? RepurposedMedicineDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
