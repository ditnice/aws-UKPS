namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// Therapeutic area hierarchy — medicines only.
/// 20 top-level areas, 172 total nodes including sub-areas.
/// </summary>
internal sealed class TherapeuticArea
{
    public int Id { get; set; }

    /// <summary>Null for top-level areas.</summary>
    public int? ParentId { get; set; }

    public required string Label { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public TherapeuticArea? Parent { get; set; }
    public ICollection<TherapeuticArea> Children { get; set; } = [];
}
