namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>The task list for a record's latest revision.</summary>
public sealed record RecordTaskListDto
{
    /// <summary>The record this task list belongs to.</summary>
    public required int RecordId { get; init; }

    /// <summary>The organisation that owns the record; used for links back to the record.</summary>
    public required int OrganisationId { get; init; }

    /// <summary>Sections and their ordered tasks.</summary>
    public required IReadOnlyList<RecordTaskListSectionDto> Sections { get; init; }
}
