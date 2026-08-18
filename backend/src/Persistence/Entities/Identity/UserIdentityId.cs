namespace UKPS.Api.Persistence.Entities.Identity;

internal record struct UserIdentityId
{
    public required string Value { get; init; }

    public static UserIdentityId GenerateNew()
    {
        return new UserIdentityId { Value = Guid.CreateVersion7().ToString() };
    }

    internal static UserIdentityId Parse(string arg)
    {
        return new UserIdentityId() { Value = arg };
    }
}
