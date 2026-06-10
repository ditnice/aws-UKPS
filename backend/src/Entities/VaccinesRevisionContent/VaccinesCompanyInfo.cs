using UKPS.Api.Enums;

namespace UKPS.Api.Entities.VaccinesRevisionContent;

public class VaccinesCompanyInfo
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? IsOriginatorCompany { get; set; }

    /// <summary>Free text; conditional on IsOriginatorCompany = No.</summary>
    public string? OriginatorCompanyName { get; set; }

    public YesNoUnknown HasBeenAcquired { get; set; }

    /// <summary>Free text; conditional on HasBeenAcquired = Yes.</summary>
    public string? PreviousOwner { get; set; }

    public YesNoUnknown HasGrantFunding { get; set; }

    /// <summary>Free text; conditional on HasGrantFunding = Yes. Grant reference number or identifier.</summary>
    public string? GrantFundingIdentifier { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
