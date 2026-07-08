using System.Globalization;
using UKPS.Api.Common;

namespace UKPS.Api.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Ok_SetsIsOkTrueAndIsErrFalse()
    {
        Result<int, string> result = Result<int, string>.Ok(1);

        Assert.True(result.IsOk);
        Assert.False(result.IsErr);
    }

    [Fact]
    public void Ok_Value_ReturnsValueAndErrorIsNull()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Err_SetsIsErrTrueAndIsOkFalse()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        Assert.True(result.IsErr);
        Assert.False(result.IsOk);
    }

    [Fact]
    public void Err_Error_ReturnsErrorAndValueIsDefault()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        Assert.Equal("failure", result.Error);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void IsOk_InsideCheckedBranch_NarrowsValueToNotNull()
    {
        Result<string, string> result = Result<string, string>.Ok("value");

        if (result.IsOk)
        {
            // Compiles without '!' or a null check: [MemberNotNullWhen] narrows Value.
            Assert.Equal(5, result.Value.Length);
        }
        else
        {
            Assert.Fail("Expected an Ok result.");
        }
    }

    [Fact]
    public void Match_Ok_InvokesOnOkWithValue()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        string matched = result.Match(value => $"ok:{value}", error => $"err:{error}");

        Assert.Equal("ok:42", matched);
    }

    [Fact]
    public void Match_Err_InvokesOnErrWithError()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        string matched = result.Match(value => $"ok:{value}", error => $"err:{error}");

        Assert.Equal("err:failure", matched);
    }

    [Fact]
    public void Match_OnOkIsNull_ThrowsArgumentNullException()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(() => result.Match(null!, error => error));
    }

    [Fact]
    public void Match_OnErrIsNull_ThrowsArgumentNullException()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Assert.Throws<ArgumentNullException>(() =>
            result.Match(value => value.ToString(CultureInfo.InvariantCulture), null!)
        );
    }

    [Fact]
    public void Ok_ValueIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string, string>.Ok(null!));
    }

    [Fact]
    public void Err_ErrorIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string, string>.Err(null!));
    }

    [Fact]
    public void Default_IsOkAndIsErrAreBothFalse()
    {
        Result<int, string> result = default;

        Assert.False(result.IsOk);
        Assert.False(result.IsErr);
    }

    [Fact]
    public void Default_ValueIsDefaultAndErrorIsNull()
    {
        Result<int, string> result = default;

        Assert.Equal(0, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Default_Match_ThrowsInvalidOperationException()
    {
        Result<int, string> result = default;

        Assert.Throws<InvalidOperationException>(() =>
            result.Match(value => value.ToString(CultureInfo.InvariantCulture), error => error)
        );
    }

    [Fact]
    public void Equals_TwoOkResultsWithSameValue_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(1);

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.False(left != right);
    }

    [Fact]
    public void Equals_TwoOkResultsWithDifferentValues_AreNotEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(2);

        Assert.False(left.Equals(right));
        Assert.False(left == right);
        Assert.True(left != right);
    }

    [Fact]
    public void Equals_OkAndErrResults_AreNotEqual()
    {
        Result<int, string> ok = Result<int, string>.Ok(1);
        Result<int, string> err = Result<int, string>.Err("failure");

        Assert.False(ok.Equals(err));
        Assert.False(ok == err);
        Assert.True(ok != err);
    }

    [Fact]
    public void Equals_TwoErrResultsWithSameError_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Err("a");
        Result<int, string> right = Result<int, string>.Err("a");

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.False(left != right);
    }

    [Fact]
    public void Equals_TwoDefaultResults_AreEqual()
    {
        Result<int, string> left = default;
        Result<int, string> right = default;

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.False(left != right);
    }

    [Fact]
    public void GetHashCode_TwoEqualOkResults_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(1);

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void GetHashCode_TwoEqualErrResults_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Err("a");
        Result<int, string> right = Result<int, string>.Err("a");

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ObjectOverload_WithEquivalentResult_ReturnsTrue()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        object right = Result<int, string>.Ok(1);

        Assert.True(left.Equals(right));
    }

    [Fact]
    public void Equals_ObjectOverload_WithNonResultObject_ReturnsFalse()
    {
        Result<int, string> result = Result<int, string>.Ok(1);

        Assert.False(result.Equals("not a result"));
    }
}
