using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents an active substance name for a medicine.
/// </summary>
public sealed record MedicinesActiveSubstanceDto
{
    /// <summary>
    /// Gets the substance name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the type of the substance name.
    /// </summary>
    public required SubstanceNameType NameType { get; init; }
}
