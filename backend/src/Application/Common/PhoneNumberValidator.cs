using PhoneNumbers;

namespace UKPS.Api.Application.Common;

/// <summary>
/// Validates phone numbers using Google's libphonenumber.
/// </summary>
internal static class PhoneNumberValidator
{
    private static readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    /// <summary>
    /// Determines whether <paramref name="telephoneNumber"/> is a valid phone number.
    /// </summary>
    /// <param name="telephoneNumber">The phone number to validate.</param>
    /// <param name="regionCode">The assumed region code when <paramref name="telephoneNumber"/>
    /// doesn't specify one a country code. Default is "GB".
    /// </param>
    public static bool IsValid(string telephoneNumber, string regionCode = "GB")
    {
        if (string.IsNullOrWhiteSpace(telephoneNumber))
        {
            return false;
        }

        try
        {
            PhoneNumber parsed = _phoneNumberUtil.Parse(telephoneNumber, regionCode);
            return _phoneNumberUtil.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether <paramref name="mobileNumber"/> is a valid mobile number for
    /// Cognito SMS MFA challenge, and if so, reformats as an E.164 string.
    /// </summary>
    /// <param name="mobileNumber">The mobile number to validate.</param>
    /// <param name="e164Number">
    /// When this method returns <see langword="true"/>, <paramref name="mobileNumber"/>
    /// reformatted as E.164 (e.g. "+447911123456"); otherwise <see langword="null"/>.
    /// </param>
    /// <param name="regionCode">The assumed region code when <paramref name="mobileNumber"/>
    /// doesn't specify one a country code. Default is "GB".
    /// </param>
    public static bool IsValidSmsNumber(
        string mobileNumber,
        out string? e164Number,
        string regionCode = "GB"
    )
    {
        e164Number = null;

        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            return false;
        }

        try
        {
            PhoneNumber parsed = _phoneNumberUtil.Parse(mobileNumber, regionCode);

            if (!_phoneNumberUtil.IsValidNumber(parsed))
            {
                return false;
            }

            PhoneNumberType numberType = _phoneNumberUtil.GetNumberType(parsed);
            if (numberType is not (PhoneNumberType.MOBILE or PhoneNumberType.FIXED_LINE_OR_MOBILE))
            {
                return false;
            }

            e164Number = _phoneNumberUtil.Format(parsed, PhoneNumberFormat.E164);
            return true;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
