using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesDevelopmentBackground
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? IsRepurposedMedicine { get; set; }

    /// <summary>
    /// Differences from the current licensed indications. Conditional on IsRepurposedMedicine = Yes.
    /// </summary>
    public string? RepurposedMedicineDetails { get; set; }

    public YesNoUnknown? IsOriginatorCompany { get; set; }

    /// <summary>Free text; conditional on IsOriginatorCompany = No.</summary>
    public string? OriginatorCompanyName { get; set; }

    public YesNoUnknown? IsCoMarketed { get; set; }

    /// <summary>Free text; conditional on IsCoMarketed = Yes.</summary>
    public string? CoMarketingCompanyName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
