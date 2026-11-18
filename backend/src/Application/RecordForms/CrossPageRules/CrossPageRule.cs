using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.CrossPageRules;

/// <summary>
/// A rule between answers on different pages. These never block a page save, so a draft can break
/// them, but they are checked on check your answers and again on Submit.
/// </summary>
internal abstract record CrossPageRule(string Id)
{
    /// <summary>The questions the rule reads, which are also the questions it reports errors on.</summary>
    public abstract IReadOnlyList<string> QuestionIds { get; }

    /// <summary>
    /// The problems with the answers, each on one of <see cref="QuestionIds"/>; none when the rule
    /// passes or doesn't apply.
    /// </summary>
    public abstract IEnumerable<(string QuestionId, string Message)> Check(
        FormModel model,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability
    );

    /// <summary>The problems with the rule's definition against the form, for the startup check.</summary>
    public virtual IEnumerable<string> Problems(FormModel model) =>
        QuestionIds
            .Where(id => model.FindQuestion(id) is null)
            .Select(id =>
                $"Cross-page rule '{Id}' names question '{id}', which is not in the form."
            );
}
