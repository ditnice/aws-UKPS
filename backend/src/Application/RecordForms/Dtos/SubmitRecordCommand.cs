using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>Submits a record's draft revision, as it was when check your answers loaded.</summary>
public sealed record SubmitRecordCommand
{
    /// <summary>The form version check your answers was loaded with.</summary>
    [Required]
    public required string FormVersion { get; init; }

    /// <summary>
    /// The hash of the whole revision check your answers was loaded with. If anything in the
    /// revision has changed since, the submission is rejected.
    /// </summary>
    [Required]
    public required string RevisionHash { get; init; }
}
