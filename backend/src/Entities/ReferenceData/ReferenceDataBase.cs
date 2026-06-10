using UKPS.Api.Enums;

namespace UKPS.Api.Entities.ReferenceData;

public abstract class ReferenceDataBase
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;
    public bool IsArchived { get; set; }
}
