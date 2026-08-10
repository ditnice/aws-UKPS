namespace UKPS.Api.Persistence.Entities.Identity;

internal class UserOnboardingRecord
{
    public required Guid SetupToken { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public DateTime? ConsumedAt { get; private set; }

    public User? User { get; init; }
    public int UserId { get; init; }

    internal void MarkAsConsumed(DateTime dateTime)
    {
        ConsumedAt = dateTime;
    }

    internal SetupTokenState GetCurrentState(DateTime currentTime, TimeSpan expiryTime)
    {
        // TODO URP 415 - Device how/if we are going to handle this corrupted entity gracefully.
        if (currentTime < CreatedAt)
        {
            throw new ArgumentException(
                "Current time cannot be earlier than the CreatedAt datetime.",
                nameof(currentTime)
            );
        }

        if (ConsumedAt is not null)
        {
            return SetupTokenState.Consumed;
        }

        if (currentTime > CreatedAt.Add(expiryTime))
        {
            return SetupTokenState.Expired;
        }

        return SetupTokenState.Valid;
    }
}
