namespace UKPS.Api.Persistence.Entities.Identity;

/// <summary>
/// Represents a unique username used to identify a user in Amazon Cognito.
/// </summary>
public readonly record struct CognitoUsername
{
    /// <summary>
    /// Gets the value of the Cognito username.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Returns the string representation of the Cognito username.
    /// </summary>
    /// <returns>The value of the Cognito username.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Generates a new unique Cognito username.
    /// </summary>
    /// <returns>A new <see cref="CognitoUsername"/> with a unique value.</returns>
    public static CognitoUsername GenerateNew()
    {
        return new CognitoUsername { Value = $"cu_{Guid.CreateVersion7()}" };
    }

    /// <summary>
    /// Creates a Cognito username from the specified value.
    /// </summary>
    /// <param name="arg">The value of the Cognito username.</param>
    /// <returns>A <see cref="CognitoUsername"/> containing the specified value.</returns>
    internal static CognitoUsername Parse(string arg)
    {
        return new CognitoUsername() { Value = arg };
    }
}
