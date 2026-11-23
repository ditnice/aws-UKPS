namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// The kind of a condition that decides whether a question applies. Part of the HTTP API
/// contract: the member names are sent to the frontend, so never rename or remove one.
/// </summary>
public enum ConditionKind
{
    /// <summary>The trigger question's answer includes one of the condition's values.</summary>
    AnswerIn = 0,
}
