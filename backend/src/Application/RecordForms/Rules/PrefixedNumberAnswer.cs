namespace UKPS.Api.Application.RecordForms.Rules;

/// <summary>
/// The rules for a <c>PrefixedNumber</c> answer: one string made of one of the question's
/// prefixes followed by digits. The frontend mirrors this in <c>lib/dynamic-form/prefixedNumber.ts</c>.
/// </summary>
internal static class PrefixedNumberAnswer
{
    private const int MaxDigits = 5;

    /// <summary>
    /// The value to save and the message describing why it is invalid. A prefix with no digits is
    /// saved as blank, so it is never an error.
    /// </summary>
    /// <param name="prefixes">The prefixes offered.</param>
    /// <param name="digits">The exact number of digits needed, or null for up to 5.</param>
    /// <param name="value">The answer.</param>
    public static (string? Value, string? Error) Evaluate(
        IReadOnlyList<string> prefixes,
        int? digits,
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

        if (!number.All(char.IsAsciiDigit) || number.All(c => c == '0'))
        {
            return (value, RecordFormMessages.IdDigitsOnly);
        }

        if (digits is { } exact)
        {
            return (value, number.Length != exact ? RecordFormMessages.IdDigits(exact) : null);
        }

        return (value, number.Length > MaxDigits ? RecordFormMessages.IdOutOfRange : null);
    }
}
