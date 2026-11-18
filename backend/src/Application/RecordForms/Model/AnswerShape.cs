namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>The JSON shape of a question's answer on the wire.</summary>
internal enum AnswerShape
{
    /// <summary>A string, or null when unanswered.</summary>
    Single = 0,

    /// <summary>An array of strings, empty when unanswered. Order does not matter.</summary>
    Multi = 1,

    /// <summary>An array of strings, empty when unanswered. Order matters.</summary>
    List = 2,

    /// <summary>
    /// An array of items, empty when unanswered. Each item is an object keyed by child question
    /// ID. Order matters.
    /// </summary>
    Group = 3,
}
