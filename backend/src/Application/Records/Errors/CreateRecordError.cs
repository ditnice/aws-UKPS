namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents an error that can occur when creating a record.
/// </summary>
public abstract record CreateRecordError
{
    /// <summary>
    /// Indicates that the current user is not authorised to create a record
    /// for the specified organisation.
    /// </summary>
    public sealed record NotAuthorised : CreateRecordError;

    /// <summary>
    /// Indicates that the specified organisation does not exist.
    /// </summary>
    public sealed record OrganisationDoesNotExist : CreateRecordError;

    internal TResult Match<TResult>(
        Func<NotAuthorised, TResult> notAuthorised,
        Func<OrganisationDoesNotExist, TResult> organisationDoesNotExist
    )
    {
        return this switch
        {
            NotAuthorised notAuthorisedError => notAuthorised(notAuthorisedError),
            OrganisationDoesNotExist organisationDoesNotExistError => organisationDoesNotExist(
                organisationDoesNotExistError
            ),
            _ => throw new NotSupportedException(),
        };
    }
}
