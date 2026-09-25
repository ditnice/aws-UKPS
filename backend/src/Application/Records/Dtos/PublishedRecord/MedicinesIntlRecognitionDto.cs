using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the international recognition section of a medicine record.
/// </summary>
public sealed record MedicinesIntlRecognitionDto
{
    /// <summary>
    /// Gets the International Recognition Procedure route.
    /// </summary>
    public ReferenceDataDto? IrpRoute { get; init; }

    /// <summary>
    /// Gets whether international conditional approval is anticipated.
    /// </summary>
    public YesNoUnknown? IntlConditionalApprovalAnticipated { get; init; }

    /// <summary>
    /// Gets the international submission date.
    /// </summary>
    public RegulatoryDateDto? IntlSubmissionDate { get; init; }

    /// <summary>
    /// Gets the international licence date.
    /// </summary>
    public RegulatoryDateDto? IntlLicenceDate { get; init; }
}
