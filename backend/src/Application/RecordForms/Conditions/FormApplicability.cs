using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// Works out which pages and questions apply from a revision's answers, and clears the answers
/// of those that do not.
/// </summary>
internal static class FormApplicability
{
    /// <summary>The applicability of the form's pages and questions, worked out as asked.</summary>
    /// <param name="model">The form.</param>
    /// <param name="answers">
    /// Answers keyed by question ID. Must include every trigger of the pages and questions asked
    /// about, which the rows of <see cref="FormModel.EntitiesToRead"/> or
    /// <see cref="FormModel.EntitiesToSave"/> provide.
    /// </param>
    /// <param name="codes">
    /// The codes of <see cref="FormModel.ReferenceTriggerTypes"/>; only needed by a form with
    /// reference data triggers.
    /// </param>
    public static Applicability Evaluate(
        FormModel model,
        IReadOnlyDictionary<string, FormAnswer> answers,
        ReferenceCodes? codes = null
    ) => new(model, answers, codes ?? ReferenceCodes.None);

    /// <summary>
    /// The answers, with each of <paramref name="questions"/> that does not apply left
    /// unanswered. Other answers are returned unchanged.
    /// </summary>
    public static Dictionary<string, FormAnswer> ClearInapplicable(
        IEnumerable<FormQuestion> questions,
        IReadOnlyDictionary<string, FormAnswer> answers,
        Applicability applicability
    )
    {
        Dictionary<string, FormAnswer> result = new(answers, StringComparer.Ordinal);
        foreach (FormQuestion question in questions.Where(q => !applicability.Applies(q)))
        {
            result[question.Id] = FormAnswer.Empty(question.Shape);
        }

        return result;
    }
}
