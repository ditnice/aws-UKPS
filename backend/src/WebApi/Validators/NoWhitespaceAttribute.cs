using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class NoWhitespaceAttribute : ValidationAttribute
{
    public NoWhitespaceAttribute()
        : base("The field cannot contain whitespace or control characters.") { }

    public override bool IsValid(object? value)
    {
        return value is not string s || !s.Any(c => char.IsWhiteSpace(c) || char.IsControl(c));
    }
}
