namespace UKPS.Api.Entities.ReferenceData;

internal abstract class ReferenceDataBase
{
    public int Id { get; set; }
    public required string Label { get; set; }
    public bool IsArchived { get; set; }
}
