namespace UKPS.Api.Persistence.Entities.Identity;

internal class UserOnboardingRecord
{
    public required Guid SetupToken { get; init; }
    public required string UserEmail { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }

    internal bool HasExpired(DateTime currentTime, TimeSpan timeSpan)
    {
        if (currentTime < CreatedAt)
        {
            throw new ArgumentException(
                "Current time cannot be earlier than the CreatedAt datetime.",
                nameof(currentTime)
            );
        }

        return currentTime > CreatedAt.Add(timeSpan);
    }
}
