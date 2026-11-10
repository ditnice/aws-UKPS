namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// The rules for a <c>PrefixedNumber</c> answer: one string made of one of the question's
/// prefixes followed by digits. The frontend mirrors this in <c>lib/dynamic-form/answers.ts</c>.
/// </summary>
internal static class PrefixedNumberAnswer
{
    /// <summary>
    /// The value to save and the message describing why it is invalid. A prefix with no digits is
    /// saved as blank, so it is never an error.
    /// </summary>
    public static (string? Value, string? Error) Evaluate(
        IReadOnlyList<string> prefixes,
        string? value
    )
    {
        if (value is null)
        {
            return (null, null);
        }

        string? prefix = prefixes.FirstOrDefault(p =>
            value.StartsWith(p, StringComparison.Ordinal)
        );
        if (prefix is null)
        {
            return (value, RecordFormMessages.SelectIdType);
        }

        string number = value[prefix.Length..];
        if (number.Length == 0)
        {
            return (null, null);
        }

        return (value, number.All(char.IsAsciiDigit) ? null : RecordFormMessages.IdDigitsOnly);
    }
}
