using UKPS.Api.Services.Results;

namespace UKPS.Api.Tests.Services.Results;

public class ResultTests
{
    private static readonly ResultError _error = new(
        "Test.Error",
        ErrorType.Validation,
        "Test error."
    );

    [Fact]
    public void Success_ReturnsSuccessfulResultWithoutError()
    {
        Result result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ReturnsFailedResultWithError()
    {
        Result result = Result.Failure(_error);

        Assert.False(result.IsSuccess);
        Assert.Same(_error, result.Error);
    }

    [Fact]
    public void SuccessfulResult_WithError_ThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new TestResult(isSuccess: true, _error)
        );

        Assert.Equal("error", exception.ParamName);
    }

    [Fact]
    public void FailedResult_WithoutError_ThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new TestResult(isSuccess: false, error: null)
        );

        Assert.Equal("error", exception.ParamName);
    }

    private sealed class TestResult(bool isSuccess, ResultError? error) : Result(isSuccess, error);
}
