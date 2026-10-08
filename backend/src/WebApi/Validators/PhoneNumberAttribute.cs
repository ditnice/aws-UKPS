using System.ComponentModel.DataAnnotations;
using UKPS.Api.Application.Common;

namespace UKPS.Api.WebApi.Validators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class PhoneNumberAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "Value must be a valid phone number.";

    public PhoneNumberAttribute()
        : base(DefaultErrorMessage) { }

    public PhoneNumberAttribute(string errorMessage)
        : base(errorMessage: errorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && PhoneNumberValidator.IsValid(str))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
