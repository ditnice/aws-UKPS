using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

internal sealed class MedicinesCompanyInfo
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? IsOriginatorCompany { get; set; }

    /// <summary>Free text; conditional on IsOriginatorCompany = No.</summary>
    public string? OriginatorCompanyName { get; set; }

    public YesNoUnknown? IsCoMarketed { get; set; }

    /// <summary>Free text; conditional on IsCoMarketed = Yes.</summary>
    public string? CoMarketingCompanyName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
