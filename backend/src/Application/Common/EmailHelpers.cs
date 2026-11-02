namespace UKPS.Api.Application.Common;

internal static class EmailHelpers
{
    public static string Normalise(string value)
    {
        // Decided to normalise to lowercase as this is more readable
#pragma warning disable CA1308 // Normalize strings to uppercase
        return value.Trim().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
    }
}
