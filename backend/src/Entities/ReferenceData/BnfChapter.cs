using UKPS.Data.Enums;

namespace UKPS.Data.Entities.ReferenceData;

/// <summary>
/// BNF chapter hierarchy — medicines only.
/// 15 top-level chapters, 125 total nodes including subsections.
/// Self-referencing for arbitrary depth.
/// </summary>
public class BnfChapter
{
    public int Id { get; set; }

    /// <summary>Null for top-level chapters.</summary>
    public int? ParentId { get; set; }

    /// <summary>e.g. "1", "1.1", "2.5"</summary>
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public BnfChapter? Parent { get; set; }
    public ICollection<BnfChapter> Children { get; set; } = [];
}
