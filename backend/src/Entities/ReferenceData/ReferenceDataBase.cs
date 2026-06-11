using UKPS.Api.Enums;

namespace UKPS.Api.Entities.ReferenceData;

internal abstract class ReferenceDataBase
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;
    public bool IsArchived { get; set; }
}
