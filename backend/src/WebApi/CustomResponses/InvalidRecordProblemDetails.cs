using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.WebApi.CustomResponses;

/// <summary>
/// The problems that stop a record being submitted. <c>errors</c> is keyed by question ID;
/// <c>fields</c> adds each problem's page and task.
/// </summary>
public sealed class InvalidRecordProblemDetails : ValidationProblemDetails
{
    /// <summary>Copies <paramref name="details"/> and adds the problems with their locations.</summary>
    /// <param name="details">The validation problem details from the problem details factory.</param>
    /// <param name="fields">Each problem with its page and task, in form order.</param>
    public InvalidRecordProblemDetails(
        ValidationProblemDetails details,
        IReadOnlyList<SubmissionErrorDto> fields
    )
        : base(details?.Errors ?? throw new ArgumentNullException(nameof(details)))
    {
        Type = details.Type;
        Title = details.Title;
        Status = details.Status;
        Detail = details.Detail;
        Instance = details.Instance;
        foreach (var (key, value) in details.Extensions)
        {
            Extensions[key] = value;
        }

        Fields = fields;
    }

    /// <summary>Identifies the problem; always <c>record_invalid</c>.</summary>
    public string Code { get; } = "record_invalid";

    /// <summary>Each problem with its page and task, in form order.</summary>
    public IReadOnlyList<SubmissionErrorDto> Fields { get; }
}
