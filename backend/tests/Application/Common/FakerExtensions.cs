using Bogus;

namespace UKPS.Api.Tests.Application.Common;

internal static class FakerExtensions
{
    public static string GetRandomSubString(
        this Faker faker,
        string initialString,
        int minLength = 1,
        int? maxLength = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(initialString);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minLength);

        var startIndex = faker.Random.Int(
            0,
            initialString.Length - Math.Min(initialString.Length, minLength)
        );
        var maxPossibleLength = initialString.Length - startIndex;
        var length = faker.Random.Int(
            minLength,
            Math.Min(maxLength ?? maxPossibleLength, maxPossibleLength)
        );

        return initialString.Substring(startIndex, length);
    }

    public static string GetRandomlyCapitalisedString(this Faker faker, string initialString)
    {
        ArgumentNullException.ThrowIfNull(initialString);

        return string.Concat(
            initialString.Select(character =>
                faker.Random.Bool()
                    ? char.ToUpperInvariant(character)
                    : char.ToLowerInvariant(character)
            )
        );
    }
}
