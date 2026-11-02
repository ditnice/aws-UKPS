namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>One page of a form. Its questions may come from several entities.</summary>
internal sealed record FormPage(
    string Id,
    string Title,
    int Order,
    string SectionId,
    string SectionTitle,
    IReadOnlyList<FormQuestion> Questions
)
{
    /// <summary>The content entities this page reads and writes.</summary>
    public IEnumerable<FormEntity> Entities => Questions.Select(static q => q.Entity).Distinct();
}
