using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

/// <summary>
/// One row per antigen. The antigen is the specific biological component
/// the immune system learns to recognise — e.g. spike protein, polysaccharide capsule,
/// haemagglutinin. Replaces 'pathogen-related components' from the original JCVI proforma.
/// </summary>
public class VaccinesAntigen
{
    public int Id { get; set; }
    public int VaccinesTechnologyId { get; set; }
    public string AntigenName { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesTechnology VaccinesTechnology { get; set; } = null!;
}
