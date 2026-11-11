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
    public const string SelectIdType = "Select ID type";
    public const string IdDigitsOnly = "ID must be a whole positive number";
    public const string IdOutOfRange = "ID must not be longer than 5 digits";
    public const string InvalidDate = "Enter a real date.";
    public const string ActualDateInFuture = "An actual date must be today or in the past.";
    public const string EstimatedDateInPast = "An estimated date must be in the future.";
    public const string ActualMonthInFuture = "The date must be this month or in the past.";
}
