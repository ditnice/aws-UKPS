using UKPS.Api.Application.Common;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserPasswordResetRequest
{
    public int Id { get; set; }
    public required string TargetEmailAddress
    {
        get => field;
        init => field = EmailHelpers.Normalise(value);
    }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UsedAt { get; private set; }

    private UserPasswordResetRequest() { }

    public void MarkAsUsed(DateTime dateTime)
    {
        UsedAt = dateTime;
    }

    public State GetCurrentState(DateTime currentTime, TimeSpan expiryTime)
    {
        if (currentTime < CreatedAt)
        {
            throw new ArgumentException(
                "Current time cannot be earlier than the CreatedAt datetime.",
                nameof(currentTime)
            );
        }
        if (UsedAt is not null)
        {
            return State.AlreadyUsed;
        }

        if (currentTime > CreatedAt.Add(expiryTime))
        {
            return State.Expired;
        }

        return State.Valid;
    }

    public static UserPasswordResetRequest Create(string targetEmailAddress, DateTime currentTime)
    {
        var request = new UserPasswordResetRequest()
        {
            TargetEmailAddress = targetEmailAddress,
            CreatedAt = currentTime,
        };
        return request;
    }

    public enum State
    {
        Valid = 0,
        AlreadyUsed = 1,
        Expired = 2,
    }
}
