namespace UKPS.Api.Persistence.Entities.Identity;

internal record struct CognitoUsername
{
    public required string Value { get; init; }

    public static CognitoUsername GenerateNew()
    {
        return new CognitoUsername { Value = $"id_{Guid.CreateVersion7()}" };
    }

    internal static CognitoUsername Parse(string arg)
    {
        return new CognitoUsername() { Value = arg };
    }
}
