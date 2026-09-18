namespace UKPS.Api.Application.Users.Errors;

/// <summary>
/// Represents an error that can occur when updating the current user's organisation.
/// </summary>
public abstract record UpdateCurrentOrganisationError
{
    /// <summary>
    /// Represents the error when the provided organisation is not valid.
    /// </summary>
    public sealed record ProvidedOrganisationWasNotValid : UpdateCurrentOrganisationError;

    internal TResult Match<TResult>(
        Func<ProvidedOrganisationWasNotValid, TResult> providedOrganisationWasNotValid
    )
    {
        ArgumentNullException.ThrowIfNull(providedOrganisationWasNotValid);

        return this switch
        {
            ProvidedOrganisationWasNotValid e => providedOrganisationWasNotValid(e),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(UpdateCurrentOrganisationError)} subtype: {GetType().Name}."
            ),
        };
    }
}
