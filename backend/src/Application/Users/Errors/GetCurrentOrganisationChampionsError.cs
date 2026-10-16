namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error retrieving the current organisation's champion contacts.
/// </summary>
public abstract record GetCurrentOrganisationChampionsError
{
    /// <summary>
    /// The current user is not allowed to retrieve the champion contacts.
    /// </summary>
    internal sealed record NotAllowed : GetCurrentOrganisationChampionsError;
}
