using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

/// <summary>
/// Alternative names, synonyms, and prior codes for this vaccine candidate.
/// Covers code names, historical names, partner organisation names, and registry identifiers.
/// </summary>
public class VaccinesCompanyCode
{
    public int Id { get; set; }
    public int VaccinesProductDetailId { get; set; }
    public string Name { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesProductDetail VaccinesProductDetail { get; set; } = null!;
}
