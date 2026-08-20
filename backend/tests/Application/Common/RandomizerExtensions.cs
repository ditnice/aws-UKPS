using Bogus;

namespace UKPS.Api.Tests.Application.Common;

internal static class RandomizerExtensions
{
    public static string RandomSubstring(this Randomizer random, string value)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(value);

        var start = random.Int(0, value.Length - 1);
        var length = random.Int(1, value.Length - start);

        return value.Substring(start, length);
    }

    public static string RandomizeCharacterCasing(this Randomizer random, string value)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(value);

        return string.Concat(
            value.Select(c =>
            {
                if (!char.IsLetter(c))
                    return c;

                return random.Bool() ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
            })
        );
    }
}
