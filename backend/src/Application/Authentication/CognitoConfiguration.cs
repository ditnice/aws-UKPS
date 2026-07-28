namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Represents the configuration settings required to connect to Amazon Cognito.
/// </summary>
public sealed record CognitoConfiguration
{
    /// <summary>
    /// Gets the configuration section name used to bind Cognito settings.
    /// </summary>
    public const string SectionName = "cognito";

    /// <summary>
    /// Gets the optional service URL used when connecting to Cognito.
    /// This can be used to override the default Cognito endpoint, for example when testing locally.
    /// </summary>
    public Uri? ServiceUrl { get; init; }

    /// <summary>
    /// Gets the AWS access key used to authenticate requests to Cognito.
    /// </summary>
    public required string AccessKey { get; init; }

    /// <summary>
    /// Gets the AWS secret key used to authenticate requests to Cognito.
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// Gets the Cognito application client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the AWS region containing the Cognito user pool.
    /// </summary>
    public required string Region { get; init; }
}
