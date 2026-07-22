using Shouldly;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Tests.Utilities.AssertionHelpers;

internal static class ResultAssertionExtensions
{
    public static TValue ShouldBeSuccess<TValue, TError>(this Result<TValue, TError> result)
        where TValue : notnull
        where TError : notnull
    {
        result.IsOk.ShouldBeTrue();
        return result.Value;
    }

    public static void ShouldBeSuccess<TError>(this Result<TError> result)
        where TError : notnull
    {
        result.IsOk.ShouldBeTrue();
    }

    public static TError ShouldBeError<TError>(this IResult<TError> result)
        where TError : notnull
    {
        result.IsErr.ShouldBeTrue();
        return result.Error!;
    }
}
