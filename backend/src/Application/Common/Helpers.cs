namespace UKPS.Api.Application.Common;

/// <summary>
/// Provides common helper utilities.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Escapes special characters in a SQL LIKE pattern.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string safe for use in a LIKE clause.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string EscapeLikePattern(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
