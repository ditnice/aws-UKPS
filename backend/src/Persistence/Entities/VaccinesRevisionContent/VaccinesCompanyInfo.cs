using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

internal sealed class VaccinesCompanyInfo
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNo? IsOriginatorCompany { get; set; }

    /// <summary>
    /// Free text; conditional on IsOriginatorCompany = No.
    /// Covers originator company name, acquisition history, and licensing history.
    /// </summary>
    public string? OriginatorDetails { get; set; }

    public YesNoUnknown? HasGrantFunding { get; set; }

    /// <summary>Free text; conditional on HasGrantFunding = Yes. Grant reference number or identifier.</summary>
    public string? GrantFundingIdentifier { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
