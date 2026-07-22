namespace UKPS.Api.Persistence.Entities.Identity;

internal class UserOnboardingRecord
{
    public int Id { get; init; }
    public required string UserEmail { get; init; }
    public required Guid SetupToken { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
}
