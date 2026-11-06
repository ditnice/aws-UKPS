using UKPS.Api.Application.RecordForms.Conditions;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>One page of a form. Its questions may come from several entities.</summary>
internal sealed record FormPage(
    string Id,
    string Title,
    int Order,
    string SectionId,
    string SectionTitle,
    string TaskId,
    string TaskTitle,
    IReadOnlyList<FormQuestion> Questions,
    bool CommercialInConfidence = false
)
{
    /// <summary>When set, the page and all its questions apply only while it is met.</summary>
    public FormCondition? ShowWhen { get; init; }

    /// <summary>The content entities this page reads and writes.</summary>
    public IEnumerable<FormEntity> Entities => Questions.Select(static q => q.Entity).Distinct();
}
