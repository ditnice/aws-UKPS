using Microsoft.AspNetCore.Mvc;

namespace UKPS.Api.WebApi.CustomResponses;

/// <summary>
/// A submission or selected-organisation conflict. Schema only: runtime responses use the
/// problem details factory and extensions to preserve tracing metadata.
/// </summary>
public sealed class SubmissionConflictProblemDetails : ProblemDetails
{
    /// <summary>
    /// The domain code (record_not_submittable, form_version_changed or record_changed),
    /// or the authentication code SelectedOrganisationMismatch.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>The organisation requested, when it differs from the selected organisation.</summary>
    public int? OrganisationId { get; init; }
}
