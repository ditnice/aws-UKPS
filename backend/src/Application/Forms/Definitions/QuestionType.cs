namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// The kind of input a question renders as. Serialised to the client in camelCase.
/// </summary>
internal enum QuestionType
{
    Textarea = 0,
    Radio = 1,
    Checkbox = 2,
    Select = 3,
}
