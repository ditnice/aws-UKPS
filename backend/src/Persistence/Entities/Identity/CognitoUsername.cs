namespace UKPS.Api.Persistence.Entities.Identity;

internal record struct CognitoUsername
{
    public required string Value { get; init; }

    public override string ToString() => Value;

    public static CognitoUsername GenerateNew()
    {
        return new CognitoUsername { Value = $"cu_{Guid.CreateVersion7()}" };
    }

    internal static CognitoUsername Parse(string arg)
    {
        return new CognitoUsername() { Value = arg };
    }
}
