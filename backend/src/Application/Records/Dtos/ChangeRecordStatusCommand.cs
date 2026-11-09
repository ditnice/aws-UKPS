using System.ComponentModel.DataAnnotations;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Contains why a record is being put on hold, resumed or archived.
/// </summary>
public record ChangeRecordStatusCommand
{
    /// <summary>
    /// Gets why the status is changing. Each status change accepts only its own reasons.
    /// </summary>
    [Required]
    public RecordStatusChangeReason? Reason { get; init; }

    /// <summary>
    /// Gets further details about the change. Required when the reason is
    /// <see cref="RecordStatusChangeReason.Other"/>.
    /// </summary>
    public string? Note { get; init; }
}
