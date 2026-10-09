namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>A page of questions, saved as a unit.</summary>
/// <param name="Id">URL slug, e.g. <c>bnf-chapter</c>.</param>
/// <param name="Title">Short page title, used for the browser title.</param>
/// <param name="SectionId">The owning section's ID.</param>
/// <param name="SectionTitle">The owning section's title, shown as the page caption.</param>
/// <param name="Questions">The page's questions, in display order.</param>
internal sealed record PageDefinition(
    string Id,
    string Title,
    string SectionId,
    string SectionTitle,
    IReadOnlyList<QuestionDefinition> Questions
);
