using Shouldly;
using UKPS.Api.Common;

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

    public static TError ShouldBeError<TValue, TError>(this Result<TValue, TError> result)
        where TValue : notnull
        where TError : notnull
    {
        result.IsErr.ShouldBeTrue();
        return result.Error;
    }
}
