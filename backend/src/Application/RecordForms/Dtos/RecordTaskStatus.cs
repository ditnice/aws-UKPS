namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>
/// Represents the status of a task in a record's task list, derived from the answers saved on the
/// record's latest revision.
/// </summary>
public enum RecordTaskStatus
{
    /// <summary>
    /// No answer has been saved for the task.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// At least one answer is saved, but one or more required questions are unanswered.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Every required question has a saved answer. Unanswered optional questions do not prevent
    /// completion.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// No question in the task applies to this record, for example Promising Innovative Medicine
    /// (PIM) and Early Access to Medicines Scheme (EAMS) on a biosimilar. Nothing to complete, and
    /// the task has no page to open.
    /// </summary>
    NotApplicable = 3,

    /// <summary>
    /// The task's first page depends on a question that has not been answered yet, for example
    /// PIM and EAMS before the technology status is. The task has no page to open yet.
    /// </summary>
    CannotStart = 4,
}
