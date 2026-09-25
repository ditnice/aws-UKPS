namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the global submission section of a medicine record.
/// </summary>
public sealed record MedicinesGlobalSubmissionDto
{
    /// <summary>
    /// Gets the region of the first global submission.
    /// </summary>
    public string? GlobalFirstSubmissionRegion { get; init; }

    /// <summary>
    /// Gets the date of the first global submission.
    /// </summary>
    public RegulatoryDateDto? GlobalSubmissionActualDate { get; init; }
}
