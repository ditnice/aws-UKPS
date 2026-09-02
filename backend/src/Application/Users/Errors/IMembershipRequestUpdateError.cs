namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when updating a membership request.
/// </summary>
public interface IMembershipRequestUpdateError
{
    /// <summary>
    /// Represents an error indicating that the current user is not allowed
    /// to perform the requested membership request update.
    /// </summary>
    public interface INotAllowed : IMembershipRequestUpdateError;

    /// <summary>
    /// Represents an error indicating that the requested membership request
    /// could not be found.
    /// </summary>
    public interface IRequestNotFound : IMembershipRequestUpdateError;

    internal TResult Match<TResult>(
        Func<INotAllowed, TResult> notAllowed,
        Func<IRequestNotFound, TResult> requestNotFound
    )
    {
        return this switch
        {
            INotAllowed e => notAllowed(e),
            IRequestNotFound e => requestNotFound(e),
            _ => throw new InvalidOperationException(
                $"Unknown {nameof(IMembershipRequestUpdateError)} type: {GetType().Name}"
            ),
        };
    }
}
