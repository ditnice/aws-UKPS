using UKPS.Data.Enums;

namespace UKPS.Data.Entities.MedicinesRevisionContent;

public class MedicinesPatientIdentification
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? ScreeningRequired { get; set; }

    /// <summary>Conditional on ScreeningRequired = Yes.</summary>
    public string? ScreeningDetails { get; set; }

    public YesNoUnknown? UrgentIdentificationRequired { get; set; }

    /// <summary>Conditional on UrgentIdentificationRequired = Yes.</summary>
    public string? UrgentIdentificationDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
