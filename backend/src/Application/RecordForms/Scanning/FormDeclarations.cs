using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Everything the scanner read: the form layouts and the annotated entities.</summary>
internal sealed class FormDeclarations
{
    private readonly Dictionary<string, PagePlacement> _pages = new(StringComparer.Ordinal);

    public FormDeclarations(
        IReadOnlyList<FormLayout> layouts,
        IReadOnlyList<EntityDeclaration> entities
    )
    {
        Layouts = layouts;
        Entities = entities;

        // A page ID declared twice is reported by the validator; the first declaration wins here.
        foreach (FormLayout layout in layouts)
        {
            foreach (FormLayoutSection section in layout.Sections)
            {
                foreach (FormLayoutTask task in section.Tasks)
                {
                    foreach (FormLayoutPage page in task.Pages)
                    {
                        _pages.TryAdd(
                            page.Id,
                            new PagePlacement(layout.RecordType, section, task, page)
                        );
                    }
                }
            }
        }

        Placed =
        [
            .. entities.SelectMany(entity =>
                entity
                    .Questions.Select(question =>
                        Place(question) is { } placement
                            ? new PlacedQuestion(entity, question, placement)
                            : null
                    )
                    .OfType<PlacedQuestion>()
            ),
        ];
    }

    public IReadOnlyList<FormLayout> Layouts { get; }

    public IReadOnlyList<EntityDeclaration> Entities { get; }

    /// <summary>
    /// Every question whose page a layout declares, in entity and declaration order. Questions
    /// naming an undeclared page are left out; the validator reports them.
    /// </summary>
    public IReadOnlyList<PlacedQuestion> Placed { get; }

    /// <summary>Where the question's page puts it, or null if no layout declares the page.</summary>
    public PagePlacement? Place(QuestionDeclaration question) =>
        _pages.GetValueOrDefault(question.PageId);

    /// <summary>The forms the entity's pages are in.</summary>
    public RecordType[] RecordTypesOf(EntityDeclaration entity) =>
        Placed
            .Where(p => p.Entity == entity)
            .Select(static p => p.RecordType)
            .Distinct()
            .Order()
            .ToArray();
}
