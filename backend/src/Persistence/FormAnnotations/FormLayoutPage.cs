namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>A page within a <see cref="FormLayoutTask"/>.</summary>
/// <param name="Id">
/// The page's ID: a lowercase slug such as <c>bnf-chapter</c> (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>),
/// unique across every form. It is the last segment of the page URL, so do not change a released
/// ID, and do not use <c>task-list</c>, which is the task list's URL.
/// <see cref="FormPageAttribute"/> names it.
/// </param>
/// <param name="Title">
/// The page title, shown as the heading when the page has more than one question (a page with
/// one question uses the question's label instead). Must not be blank.
/// </param>
/// <param name="Order">
/// Position of the page within its task. No two pages in a task may share one.
/// </param>
/// <param name="CommercialInConfidence">
/// Whether the page shows the "Commercial in confidence" badge.
/// </param>
internal sealed record FormLayoutPage(
    string Id,
    string Title,
    int Order,
    bool CommercialInConfidence = false
);
