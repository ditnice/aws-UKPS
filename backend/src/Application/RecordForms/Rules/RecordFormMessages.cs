namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>Server messages for problems the rule attributes do not describe.</summary>
internal static class RecordFormMessages
{
    public const string Missing =
        "Provide an answer for this question, or null if it is unanswered";

    public const string NotOnPage = "This is not a question on this page";
    public const string WrongShape = "The answer is not in the expected format";
    public const string InvalidOption = "Select a valid option";
    public const string Duplicate = "You have selected the same option more than once";
    public const string SelectIdType = "Select ID type";
    public const string IdDigitsOnly = "ID must be a whole positive number";
    public const string IdOutOfRange = "ID must not be longer than 5 digits";
    public const string InvalidDate = "Enter a real date";
    public const string ActualMonthInFuture = "The date must be this month or in the past";

    // A PrecisionDate's messages name its date, for example "submission". The page also says
    // "Enter the actual submission date" when a part is blank, which never reaches the server.
    public static string DateNotReal(bool actual, string name) =>
        $"{(actual ? "Actual" : "Estimated")} date of {name} must be a real date";

    public static string ActualDateInFuture(string name) =>
        $"Actual date of {name} must be in the past";

    public static string EstimatedDateInPast(string name) =>
        $"Estimated date of {name} must be in the future";
}
