using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.WebApi.CustomResponses;

/// <summary>
/// The problems that stop a record being submitted. <c>errors</c> is keyed by question ID;
/// <c>fields</c> adds each problem's page and task.
/// </summary>
/// <remarks>
/// Documents the response shape for the OpenAPI schema only. The response itself is the problem
/// details factory's <see cref="ValidationProblemDetails"/>, with <c>code</c> and <c>fields</c>
/// added as extensions, so it keeps the factory's defaults such as <c>traceId</c>.
/// </remarks>
public sealed class InvalidRecordProblemDetails : ValidationProblemDetails
{
    /// <summary>The <c>code</c> of this problem.</summary>
    public const string RecordInvalidCode = "record_invalid";

    /// <summary>The problem details member holding <see cref="Fields"/>.</summary>
    public const string FieldsExtension = "fields";

    /// <summary>Identifies the problem; always <c>record_invalid</c>.</summary>
    public string Code { get; init; } = RecordInvalidCode;

    /// <summary>Each problem with its page and task, in form order.</summary>
    public IReadOnlyList<SubmissionErrorDto> Fields { get; init; } = [];
}
