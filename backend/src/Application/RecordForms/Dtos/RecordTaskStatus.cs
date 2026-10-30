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
}
