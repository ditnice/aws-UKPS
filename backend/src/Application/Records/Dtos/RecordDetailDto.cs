using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>A record's overview and every answer that applies on its latest revision.</summary>
public sealed record RecordDetailDto
{
    /// <summary>Gets the record's status, permitted actions and identifying details.</summary>
    public required RecordOverviewDto Overview { get; init; }

    /// <summary>
    /// Gets the sections with a question that applies, in form order; empty when the record type
    /// has no content form.
    /// </summary>
    public required IReadOnlyList<RecordAnswersSectionDto> Sections { get; init; }
}
