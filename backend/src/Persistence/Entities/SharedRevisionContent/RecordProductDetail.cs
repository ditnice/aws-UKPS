namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Names and identifiers for the product.</summary>
internal sealed class RecordProductDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// Internal code or working name e.g. ABC-123, mRNA-1273, BNT162b2.
    /// Primary identifier at pipeline stage.
    /// </summary>
    public required string CompanyCode { get; set; }

    public string? BrandedName { get; set; }

    /// <summary>
    /// Short human-readable label to identify this record on the homepage.
    /// e.g. Chronic hepatitis C in adults, RSV — adults 60+.
    /// </summary>
    public required string RecordTitle { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }

    /// <summary>
    /// Generic names (medicines only) and other names, codes, or synonyms.
    /// </summary>
    public ICollection<RecordNameAndIdentifier> NamesAndIdentifiers { get; set; } = [];
}
