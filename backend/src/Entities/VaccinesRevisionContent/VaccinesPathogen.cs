namespace UKPS.Api.Entities.VaccinesRevisionContent;

/// <summary>
/// One row per target pathogen. Handles both multivalent vaccines (e.g. MMR = 3 rows)
/// and polyvalent vaccines (multiple strains, one disease).
/// </summary>
internal sealed class VaccinesPathogen
{
    public int Id { get; set; }
    public int VaccinesDiseaseDetailId { get; set; }

    /// <summary>e.g. Measles virus, Streptococcus pneumoniae, RSV</summary>
    public required string PathogenName { get; set; }

    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesDiseaseDetail? VaccinesDiseaseDetail { get; set; }
}
