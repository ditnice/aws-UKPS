using Bogus;

namespace UKPS.Api.Tests.Application.Common;

internal static class FakerExtensions
{
    public static string GetRandomSubString(this Faker faker, string initialString)
    {
        ArgumentException.ThrowIfNullOrEmpty(initialString);

        var startIndex = faker.Random.Int(0, initialString.Length - 1);
        var length = faker.Random.Int(1, initialString.Length - startIndex);

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
