namespace UKPS.Api.Persistence.Entities.Identity;

internal class UserOnboardingRecord
{
    public required Guid SetupToken { get; init; }
    public required string UserEmail { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
}
