using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// The rows and answers a page request loaded, and which pages and questions apply given them.
/// </summary>
/// <param name="Rows">The revision's row of each loaded entity, or null when it has none yet.</param>
/// <param name="Answers">The answer of every question stored in <paramref name="Rows"/>.</param>
/// <param name="Applicability">What applies, given <paramref name="Answers"/>.</param>
/// <param name="Codes">The codes of the form's reference data triggers.</param>
internal sealed record LoadedAnswers(
    Dictionary<FormEntity, object?> Rows,
    Dictionary<string, FormAnswer> Answers,
    Applicability Applicability,
    ReferenceCodes Codes
);
