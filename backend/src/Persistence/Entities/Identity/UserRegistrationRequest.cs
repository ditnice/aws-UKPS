namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserRegistrationRequest
{
    public int Id { get; init; }
    public required int OrganisationId { get; init; }
    public required string FullName { get; init; }
    public required string WorkEmail { get; init; }
    public required string PhoneNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
    public int? RejectedBy { get; init; }
    public int? ApprovedByUserId { get; init; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    // Navigation
    public Organisation? Organisation { get; init; }
    public User? RejectedByUser { get; private set; }
    public User? ApprovedByUser { get; private set; }

    internal void Approve(User currentUser, DateTime dateTime)
    {
        ValidateThatStateIsCurrentlyPending();

        ApprovedByUser = currentUser;
        ApprovedAt = dateTime;
    }

    internal void Reject(User currentUser, DateTime dateTime)
    {
        ValidateThatStateIsCurrentlyPending();

        RejectedByUser = currentUser;
        RejectedAt = dateTime;
    }

    internal State GetState()
    {
        return (RejectedAt, ApprovedAt) switch
        {
            (null, not null) => State.Approved,
            (not null, null) => State.Rejected,
            (null, null) => State.Pending,
            _ => throw new InvalidOperationException(
                $"The membership request [Id:{Id}] has an invalid state because both RejectedAt and ApprovedAt are set."
            ),
        };
    }

    private void ValidateThatStateIsCurrentlyPending()
    {
        var state = GetState();
        if (state != State.Pending)
        {
            throw new InvalidOperationException(
                $"The request cannot be approved or rejected as it is currently {state}"
            );
        }
    }

    internal enum State
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }
}
