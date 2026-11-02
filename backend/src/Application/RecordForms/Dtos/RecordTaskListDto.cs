using UKPS.Api.Application.Records.Dtos;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>The task list for a record's latest revision.</summary>
public sealed record RecordTaskListDto
{
    /// <summary>The record this task list belongs to.</summary>
    public required int RecordId { get; init; }

    /// <summary>The organisation that owns the record; used for links back to the record.</summary>
    public required int OrganisationId { get; init; }

    /// <summary>The status shown to users for the record.</summary>
    public required RecordDisplayStatus DisplayStatus { get; init; }

    /// <summary>The latest revision's company code; null when it has no product details.</summary>
    public required string? CompanyCode { get; init; }

    /// <summary>The latest revision's record title; null when it has no product details.</summary>
    public required string? RecordTitle { get; init; }

    /// <summary>Sections and their ordered tasks.</summary>
    public required IReadOnlyList<RecordTaskListSectionDto> Sections { get; init; }
}
