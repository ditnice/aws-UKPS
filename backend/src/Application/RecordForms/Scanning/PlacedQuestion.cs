using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>A question declaration with its entity and where its page's task puts it.</summary>
internal sealed record PlacedQuestion(
    EntityDeclaration Entity,
    QuestionDeclaration Question,
    TaskPlacement Placement
)
{
    /// <summary>The form the question is in.</summary>
    public RecordType RecordType => Placement.RecordType;

    /// <summary>The ID of the task the question's page is in.</summary>
    public string TaskId => Placement.Task.Id;

    /// <summary>The page's position in the form: section, task, then page order.</summary>
    public (int Section, int Task, int Page) Position =>
        (Placement.Section.Order, Placement.Task.Order, Question.Page.Order);
}
