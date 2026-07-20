using System.Globalization;
using Shouldly;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Ok_SetsIsOkTrueAndIsErrFalse()
    {
        Result<int, string> result = Result<int, string>.Ok(1);

        result.IsOk.ShouldBeTrue();
        result.IsErr.ShouldBeFalse();
    }

    [Fact]
    public void Ok_Value_ReturnsValueAndErrorIsNull()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Err_SetsIsErrTrueAndIsOkFalse()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        result.IsErr.ShouldBeTrue();
        result.IsOk.ShouldBeFalse();
    }

    [Fact]
    public void Err_Error_ReturnsErrorAndValueIsDefault()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        result.Error.ShouldBe("failure");
        result.Value.ShouldBe(0);
    }

    [Fact]
    public void IsOk_InsideCheckedBranch_NarrowsValueToNotNull()
    {
        Result<string, string> result = Result<string, string>.Ok("value");

        result.IsOk.ShouldBeTrue();
        if (result.IsOk)
        {
            result.Value.Length.ShouldBe(5);
        }
    }

    [Fact]
    public void Match_Ok_InvokesOnOkWithValue()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        string matched = result.Match(value => $"ok:{value}", error => $"err:{error}");

        matched.ShouldBe("ok:42");
    }

    [Fact]
    public void Match_Err_InvokesOnErrWithError()
    {
        Result<int, string> result = Result<int, string>.Err("failure");

        string matched = result.Match(value => $"ok:{value}", error => $"err:{error}");

        matched.ShouldBe("err:failure");
    }

    [Fact]
    public void Match_OnOkIsNull_ThrowsArgumentNullException()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Should.Throw<ArgumentNullException>(() => result.Match(null!, error => error));
    }

    [Fact]
    public void Match_OnErrIsNull_ThrowsArgumentNullException()
    {
        Result<int, string> result = Result<int, string>.Ok(42);

        Should.Throw<ArgumentNullException>(() =>
            result.Match(value => value.ToString(CultureInfo.InvariantCulture), null!)
        );
    }

    [Fact]
    public void Ok_ValueIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => Result<string, string>.Ok(null!));
    }

    [Fact]
    public void Err_ErrorIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => Result<string, string>.Err(null!));
    }

    [Fact]
    public void Default_IsOkAndIsErrAreBothFalse()
    {
        Result<int, string> result = default;

        result.IsOk.ShouldBeFalse();
        result.IsErr.ShouldBeFalse();
    }

    [Fact]
    public void Default_ValueIsDefaultAndErrorIsNull()
    {
        Result<int, string> result = default;

        result.Value.ShouldBe(0);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Default_Match_ThrowsInvalidOperationException()
    {
        Result<int, string> result = default;

        Should.Throw<InvalidOperationException>(() =>
            result.Match(value => value.ToString(CultureInfo.InvariantCulture), error => error)
        );
    }

    [Fact]
    public void Equals_TwoOkResultsWithSameValue_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(1);

        left.ShouldBe(right);
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void Equals_TwoOkResultsWithDifferentValues_AreNotEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(2);

        left.ShouldNotBe(right);
        (left == right).ShouldBeFalse();
        (left != right).ShouldBeTrue();
    }

    [Fact]
    public void Equals_OkAndErrResults_AreNotEqual()
    {
        Result<int, string> ok = Result<int, string>.Ok(1);
        Result<int, string> err = Result<int, string>.Err("failure");

        ok.ShouldNotBe(err);
        (ok == err).ShouldBeFalse();
        (ok != err).ShouldBeTrue();
    }

    [Fact]
    public void Equals_TwoErrResultsWithSameError_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Err("a");
        Result<int, string> right = Result<int, string>.Err("a");

        left.ShouldBe(right);
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void Equals_TwoDefaultResults_AreEqual()
    {
        Result<int, string> left = default;
        Result<int, string> right = default;

        left.ShouldBe(right);
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_TwoEqualOkResults_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        Result<int, string> right = Result<int, string>.Ok(1);

        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void GetHashCode_TwoEqualErrResults_AreEqual()
    {
        Result<int, string> left = Result<int, string>.Err("a");
        Result<int, string> right = Result<int, string>.Err("a");

        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void Equals_ObjectOverload_WithEquivalentResult_ReturnsTrue()
    {
        Result<int, string> left = Result<int, string>.Ok(1);
        object right = Result<int, string>.Ok(1);

        left.Equals(right).ShouldBeTrue();
    }

    [Fact]
    public void Equals_ObjectOverload_WithNonResultObject_ReturnsFalse()
    {
        Result<int, string> result = Result<int, string>.Ok(1);

        result.Equals("not a result").ShouldBeFalse();
    }
}
