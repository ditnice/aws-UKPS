using UKPS.Api.Enums;

namespace UKPS.Api.Entities.VaccinesRevisionContent;

/// <summary>
/// One row per target pathogen. Handles both multivalent vaccines (e.g. MMR = 3 rows)
/// and polyvalent vaccines (multiple strains, one disease).
/// </summary>
public class VaccinesPathogen
{
    public int Id { get; set; }
    public int VaccinesDiseaseDetailId { get; set; }

    /// <summary>e.g. Measles virus, Streptococcus pneumoniae, RSV</summary>
    public string PathogenName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesDiseaseDetail VaccinesDiseaseDetail { get; set; } = null!;
}
