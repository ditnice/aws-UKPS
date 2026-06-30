namespace UKPS.Api.Entities.VaccinesRevisionContent;

/// <summary>
/// One row per adjuvant component. Supports zero, one, or multiple adjuvants.
/// An adjuvant enhances the immune response but is not itself immunogenic —
/// distinct from the antigen. e.g. AS01, aluminium hydroxide, MF59.
/// </summary>
internal sealed class VaccinesAdjuvant
{
    public int Id { get; set; }
    public int VaccinesTechnologyId { get; set; }
    public string AdjuvantName { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    // Navigation
    public VaccinesTechnology VaccinesTechnology { get; set; } = null!;
}
