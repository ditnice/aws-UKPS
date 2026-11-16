using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Every answer on a revision, what applies, and the revision hash Submit checks.</summary>
internal sealed record LoadedRevision(
    IReadOnlyDictionary<string, FormAnswer> Answers,
    Applicability Applicability,
    string Hash
);
