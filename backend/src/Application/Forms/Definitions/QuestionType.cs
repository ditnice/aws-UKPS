namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// The kind of input a form question renders as.
/// </summary>
public enum QuestionType
{
    /// <summary>Multi-line free text. The answer is a string or <c>null</c>.</summary>
    Textarea = 0,

    /// <summary>Choose one option. The answer is an option value or <c>null</c>.</summary>
    Radio = 1,

    /// <summary>Choose any number of options. The answer is an array of option values.</summary>
    Checkbox = 2,

    /// <summary>Choose one option from a dropdown. The answer is an option value or <c>null</c>.</summary>
    Select = 3,
}
