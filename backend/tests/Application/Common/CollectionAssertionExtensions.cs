using Shouldly;

namespace UKPS.Api.Tests.Application.Common;

internal static class CollectionAssertionExtensions
{
    public static void ShouldOnlyContain<T>(
        this IEnumerable<T> values,
        IEnumerable<T> expectedValueSet
    )
    {
        values.ShouldAllBe(x => expectedValueSet.Contains(x));
    }

    public static void ShouldContainSet<T>(
        this IEnumerable<T> values,
        IEnumerable<T> expectedValueSet
    )
    {
        ArgumentNullException.ThrowIfNull(expectedValueSet);
        foreach (var value in expectedValueSet)
        {
            values.ShouldContain(value);
        }
    }
}
