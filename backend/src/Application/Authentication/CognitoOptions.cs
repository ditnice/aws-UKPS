namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Represents the configuration settings required to connect to Amazon Cognito.
/// </summary>
public sealed record CognitoOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind Cognito settings.
    /// </summary>
    public const string SectionName = "cognito";

    /// <summary>
    /// Gets an optional override for the Amazon Cognito service endpoint.
    /// </summary>
    /// <remarks>
    /// When specified, this URI is used instead of the default Amazon Cognito
    /// endpoint derived from <see cref="Region"/>. This can be useful when
    /// connecting to a local or otherwise custom Cognito-compatible endpoint.
    /// </remarks>
    public Uri? ServiceUrlOverride { get; init; }

    /// <summary>
    /// Gets the Cognito application client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the Cognito application client secret.
    /// This is used to authenticate confidential clients when communicating with Amazon Cognito.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Gets the AWS region containing the Cognito user pool.
    /// </summary>
    public required string Region { get; init; }

    /// <summary>
    /// Gets the identifier of the Cognito user pool.
    /// </summary>
    public required string UserPoolId { get; init; }

    /// <summary>
    /// Gets the Amazon Cognito service endpoint.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="ServiceUrlOverride"/> when an override has been configured;
    /// otherwise, constructs the default Amazon Cognito endpoint for the configured
    /// <see cref="Region"/>.
    /// </remarks>
    public Uri ServiceUrl =>
        ServiceUrlOverride ?? new Uri($"https://cognito-idp.{Region}.amazonaws.com/");
}
