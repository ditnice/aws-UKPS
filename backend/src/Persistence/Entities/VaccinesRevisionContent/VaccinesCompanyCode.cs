namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

/// <summary>
/// Alternative names, synonyms, and prior codes for this vaccine candidate.
/// Covers code names, historical names, partner organisation names, and registry identifiers.
/// </summary>
internal sealed class VaccinesCompanyCode
{
    public int Id { get; set; }
    public int VaccinesProductDetailId { get; set; }
    public required string Name { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesProductDetail? VaccinesProductDetail { get; set; }
}
