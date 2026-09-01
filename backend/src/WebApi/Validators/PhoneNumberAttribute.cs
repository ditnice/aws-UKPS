using System.ComponentModel.DataAnnotations;
using UKPS.Api.Application.Common;

namespace UKPS.Api.WebApi.Validators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class PhoneNumberAttribute : ValidationAttribute
{
    public PhoneNumberAttribute()
        : base() { }

    public PhoneNumberAttribute(string errorMessage)
        : base(errorMessage: errorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && PhoneNumberValidator.IsValid(str))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            ErrorMessage ?? "Value must be a valid phone number.",
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
