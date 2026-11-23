using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>Decides from earlier answers whether a page or question applies.</summary>
internal abstract record FormCondition
{
    public abstract ConditionKind Kind { get; }

    /// <summary>The questions whose answers the condition reads.</summary>
    public abstract IReadOnlyList<string> TriggerQuestionIds { get; }

    /// <summary>A stable description, for the form fingerprint.</summary>
    public abstract string Description { get; }

    /// <summary>
    /// Whether the condition holds. <paramref name="answerOf"/> returns a trigger's answer, which
    /// is empty when the trigger itself does not apply.
    /// </summary>
    public abstract bool IsMet(Func<string, FormAnswer> answerOf);
}
