namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error retrieving the current organisation's champion contacts.
/// </summary>
public abstract record GetCurrentOrganisationChampionsError
{
    /// <summary>
    /// The current user is not allowed to retrieve the champion contacts.
    /// </summary>
    public sealed record NotAllowed : GetCurrentOrganisationChampionsError;

    internal TResult Match<TResult>(Func<NotAllowed, TResult> notAllowed)
    {
        ArgumentNullException.ThrowIfNull(notAllowed);

        return this switch
        {
            NotAllowed e => notAllowed(e),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(GetCurrentOrganisationChampionsError)} subtype: {GetType().Name}."
            ),
        };
    }
}
