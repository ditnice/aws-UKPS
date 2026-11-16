using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Everything the scanner read: the form layouts and the annotated entities.</summary>
internal sealed class FormDeclarations
{
    private readonly Dictionary<string, TaskPlacement> _tasks = new(StringComparer.Ordinal);

    public FormDeclarations(
        IReadOnlyList<FormLayout> layouts,
        IReadOnlyList<EntityDeclaration> entities
    )
    {
        Layouts = layouts;
        Entities = entities;

        // A task ID declared twice is reported by the validator; the first declaration wins here.
        foreach (FormLayout layout in layouts)
        {
            foreach (FormLayoutSection section in layout.Sections)
            {
                foreach (FormLayoutTask task in section.Tasks)
                {
                    _tasks.TryAdd(task.Id, new TaskPlacement(layout.RecordType, section, task));
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
    /// Every question whose page's task a layout declares, in entity and declaration order.
    /// Questions naming an undeclared task are left out; the validator reports them.
    /// </summary>
    public IReadOnlyList<PlacedQuestion> Placed { get; }

    /// <summary>Where the question's page's task puts it, or null if no layout declares the task.</summary>
    public TaskPlacement? Place(QuestionDeclaration question) =>
        _tasks.GetValueOrDefault(question.Page.Task);

    /// <summary>The forms the entity's pages are in.</summary>
    public RecordType[] RecordTypesOf(EntityDeclaration entity) =>
        Placed
            .Where(p => p.Entity == entity)
            .Select(static p => p.RecordType)
            .Distinct()
            .Order()
            .ToArray();
}
