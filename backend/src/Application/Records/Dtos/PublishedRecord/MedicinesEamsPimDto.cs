using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the Early Access to Medicines Scheme and Promising Innovative Medicine section of a
/// medicine record.
/// </summary>
public sealed record MedicinesEamsPimDto
{
    /// <summary>
    /// Gets the Promising Innovative Medicine designation status.
    /// </summary>
    public DesignationStatus? PimDesignationStatus { get; init; }

    /// <summary>
    /// Gets whether the product will be submitted to EAMS.
    /// </summary>
    public YesNoUnknown? WillSubmitToEams { get; init; }

    /// <summary>
    /// Gets the EAMS opinion decision.
    /// </summary>
    public EamsOpinionDecision? EamsOpinionDecision { get; init; }

    /// <summary>
    /// Gets the EAMS submission date.
    /// </summary>
    public RegulatoryDateDto? EamsSubmissionDate { get; init; }

    /// <summary>
    /// Gets the EAMS opinion date.
    /// </summary>
    public RegulatoryDateDto? EamsOpinionDate { get; init; }
}
