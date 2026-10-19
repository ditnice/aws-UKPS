namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// Therapeutic area hierarchy — medicines only. Top-level areas (for example "Addiction") with
/// child diseases or conditions (for example "Opioid use disorder").
/// </summary>
internal sealed class TherapeuticArea
{
    public int Id { get; set; }

    /// <summary>Null for top-level areas.</summary>
    public int? ParentId { get; set; }

    /// <summary>Display code, for example "1" for a top-level area or "1.3" for a child.</summary>
    public required string Code { get; set; }

    public required string Label { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public TherapeuticArea? Parent { get; set; }
    public ICollection<TherapeuticArea> Children { get; set; } = [];
}
