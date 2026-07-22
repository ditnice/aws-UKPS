namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

/// <summary>
/// One row per antigen. The antigen is the specific biological component
/// the immune system learns to recognise — e.g. spike protein, polysaccharide capsule,
/// haemagglutinin. Replaces 'pathogen-related components' from the original JCVI proforma.
/// </summary>
internal sealed class VaccinesAntigen
{
    public int Id { get; set; }
    public int VaccinesTechnologyId { get; set; }
    public required string AntigenName { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesTechnology? VaccinesTechnology { get; set; }
}
