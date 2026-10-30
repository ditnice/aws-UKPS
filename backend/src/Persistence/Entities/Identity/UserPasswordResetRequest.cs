using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserPasswordResetRequest
{
    public required byte[] TokenHash { get; init; }
    public required string TargetEmailAddress { get; init; }
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

    public static (UserPasswordResetRequest Request, string Token) Create(
        string targetEmailAddress,
        DateTime currentTime
    )
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var request = new UserPasswordResetRequest()
        {
            TokenHash = SHA256.HashData(tokenBytes),
            TargetEmailAddress = targetEmailAddress,
            CreatedAt = currentTime,
        };
        return (request, WebEncoders.Base64UrlEncode(tokenBytes));
    }

    public enum State
    {
        Valid = 0,
        AlreadyUsed = 1,
        Expired = 2,
    }
}
