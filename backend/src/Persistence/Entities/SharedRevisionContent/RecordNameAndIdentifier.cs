using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordNameAndIdentifier
{
    public int Id { get; set; }
    public int RecordProductDetailId { get; set; }
    public required string Name { get; set; }
    public NameAndIdentifierType NameType { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public RecordProductDetail? RecordProductDetail { get; set; }
}
