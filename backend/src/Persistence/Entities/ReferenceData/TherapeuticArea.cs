namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// Therapeutic area hierarchy — medicines only. Currently flat; supports future sub-levels.
/// 20 values.
/// </summary>
internal sealed class TherapeuticArea
{
    public int Id { get; set; }

    /// <summary>Null for top-level areas. Currently flat; structure supports future sub-levels.</summary>
    public int? ParentId { get; set; }

    public required string Label { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public TherapeuticArea? Parent { get; set; }
    public ICollection<TherapeuticArea> Children { get; set; } = [];
}
