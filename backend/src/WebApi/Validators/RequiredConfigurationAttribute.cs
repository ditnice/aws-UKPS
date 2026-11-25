using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Configuration;

namespace UKPS.Api.WebApi.Validators;

/// <summary>
/// Validates that a configuration value is present and has been replaced with a real value.
/// It works like <see cref="RequiredAttribute"/>, but it also rejects the placeholder values used
/// in <c>appsettings.json</c>, such as <see cref="FromEnvironmentVariable"/> and
/// <see cref="FromAwsSecrets"/>.
/// </summary>
/// <remarks>
/// Use this on <see cref="IAppOptions"/> properties that are bound with
/// <see cref="OptionConfigurationExtensions.BindAppOptions{T}"/>. If a placeholder is never
/// overridden by an environment variable or an AWS secret, the application fails at startup
/// instead of running with the placeholder as its value.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false
)]
internal sealed class RequiredConfigurationAttribute : RequiredAttribute
{
    /// <summary>
    /// The placeholder for values that come from an environment variable.
    /// </summary>
    public const string FromEnvironmentVariable = "FROM_ENVIRONMENT_VARIABLE";

    /// <summary>
    /// The placeholder for values that come from AWS Secrets Manager.
    /// </summary>
    public const string FromAwsSecrets = "FROM_AWS_SECRETS";

    private static readonly string[] Placeholders = [FromEnvironmentVariable, FromAwsSecrets];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ValidationResult? requiredResult = base.IsValid(value, validationContext);
        if (requiredResult != ValidationResult.Success)
        {
            return requiredResult;
        }

        string? placeholder = FindPlaceholder(value);
        if (placeholder is null)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            $"{validationContext.DisplayName} is set to the placeholder value '{placeholder}'. Provide a real value in configuration.",
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }

    private static string? FindPlaceholder(object? value)
    {
        string? text = value switch
        {
            string str => str,
            Uri uri => uri.OriginalString,
            _ => null,
        };

        if (text is null)
        {
            return null;
        }

        string trimmed = text.Trim();
        return Array.Find(
            Placeholders,
            placeholder => string.Equals(trimmed, placeholder, StringComparison.OrdinalIgnoreCase)
        );
    }
}
