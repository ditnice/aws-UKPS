using Shouldly;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Tests.Application.Common;

public class PhoneNumberValidatorTests
{
    [Theory]
    [InlineData("020 1234 5678")] // UK landline, no country code.
    [InlineData("07911 123456")] // UK mobile, no country code.
    [InlineData("+44 121 234 5678")] // UK, with country code.
    [InlineData("+33 1 42 68 53 00")] // France, with country code.
    [InlineData("+1 (212) 555-0123")] // USA, with country code.
    public void IsValid_ValidNumber_ReturnsTrue(string telephoneNumber)
    {
        PhoneNumberValidator.IsValid(telephoneNumber).ShouldBeTrue();
    }

    [Theory]
    [InlineData("not-a-phone-number")]
    [InlineData("123")]
    [InlineData("01632 960001")] // Reserved Ofcom number, rejected by libphonenumber.
    [InlineData("01 42 68 53 00")] // Correct French number, but no country code (parsed as GB).
    public void IsValid_InvalidNumber_ReturnsFalse(string telephoneNumber)
    {
        PhoneNumberValidator.IsValid(telephoneNumber).ShouldBeFalse();
    }

    [Theory]
    [InlineData("+44 7911 123456", "+447911123456")] // UK mobile.
    [InlineData("+1 202 555 0143", "+12025550143")] // US number, ambiguous mobile/landline.
    [InlineData("+33 6 12 34 56 78", "+33612345678")] // France mobile.
    [InlineData("+49 151 12345678", "+4915112345678")] // Germany mobile.
    [InlineData("+353 85 123 4567", "+353851234567")] // Ireland mobile.
    public void IsValidSmsNumber_ValidMobileWithCountryCode_ReturnsTrueAndE164Number(
        string mobileNumber,
        string expectedE164Number
    )
    {
        bool result = PhoneNumberValidator.IsValidSmsNumber(mobileNumber, out string? e164Number);

        result.ShouldBeTrue();
        e164Number.ShouldBe(expectedE164Number);
    }

    [Theory]
    [InlineData("01 42 68 53 00")] // Correct French number, but no country code supplied.
    [InlineData("+44 20 7946 0958")] // UK landline - not mobile-capable.
    [InlineData("not-a-phone-number")]
    [InlineData("")]
    [InlineData("+999 123 4567")] // Non-existent country calling code.
    public void IsValidSmsNumber_InvalidOrAmbiguousNumber_ReturnsFalse(string mobileNumber)
    {
        bool result = PhoneNumberValidator.IsValidSmsNumber(mobileNumber, out string? e164Number);

        result.ShouldBeFalse();
        e164Number.ShouldBeNull();
    }

    [Fact]
    public void IsValidSmsNumber_NoCountryCodeButValidGBNumber()
    {
        bool result = PhoneNumberValidator.IsValidSmsNumber("07911 123456", out string? e164Number);

        result.ShouldBeTrue();
        e164Number.ShouldBe("+447911123456");
    }
}
