using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

public class VaccinesProductDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Short human-readable label for the homepage.
    /// Guidance: use disease target + population pattern e.g. RSV — adults 60+.
    /// </summary>
    public string RecordTitle { get; set; } = null!;

    /// <summary>
    /// Internal code or working name e.g. mRNA-1273, BNT162b2, V116.
    /// Primary identifier at pipeline stage. Also known as candidate name in industry.
    /// </summary>
    public string CompanyCode { get; set; } = null!;

    public string? BrandedName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<VaccinesCompanyCode> CompanyCodes { get; set; } = [];
}
