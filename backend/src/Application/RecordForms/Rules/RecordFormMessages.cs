namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>Server messages for problems the rule attributes do not describe.</summary>
internal static class RecordFormMessages
{
    public const string Missing =
        "Provide an answer for this question, or null if it is unanswered.";

    public const string NotOnPage = "This is not a question on this page.";
    public const string WrongShape = "The answer is not in the expected format.";
    public const string InvalidOption = "Select a valid option.";
    public const string Duplicate = "You have selected the same option more than once.";
}
