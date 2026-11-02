namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>The JSON shape of a question's answer on the wire.</summary>
internal enum AnswerShape
{
    /// <summary>A string, or null when unanswered.</summary>
    Single = 0,

    /// <summary>An array of strings, empty when unanswered.</summary>
    Multi = 1,
}
