global using ResendVerificationEmailResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.ResendVerificationEmailError>;
global using UpdateEmailToCompletePendingEmailChangeResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.UpdateEmailToCompletePendingEmailError>;
global using VerifyPendingEmailAddressResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.VerifyPendingEmailAddressError>;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Defines the contract for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves information about the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the current user's details, including their organisation membership and role.
    /// </returns>
    Task<CurrentUserInformationDto> GetCurrentUser(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of users based on the specified criteria.
    /// </summary>
    /// <param name="getUsersQuery">The query parameters used to filter and paginate users.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> object with a paginated response of user list items
    /// or an error of type <see cref="GetUsersError"/>.
    /// </returns>
    Task<Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>> GetUsers(
        GetUsersQueryDto getUsersQuery,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="command">The details to update for the user.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> with the updated user's details, or an
    /// error of type <see cref="UpdateUserDetailsError"/> if the user's details could
    /// not be updated.
    /// </returns>
    Task<Result<UserDetailsDto, UpdateUserDetailsError>> UpdateUserDetails(
        int userId,
        UpdateUserDetailsCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the email address for an existing user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose email address is to be updated.</param>
    /// <param name="command">The email update request details.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result indicates whether the
    /// email address was updated or the reason the update failed.
    /// </returns>
    Task<Result<UpdateUserEmailError>> UpdateUserEmail(
        int userId,
        UpdateUserEmailCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Resends the account verification email to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who should receive the verification email.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a
    /// <see cref="ResendVerificationEmailResult"/> describing the outcome of the request.
    /// </returns>
    Task<ResendVerificationEmailResult> ResendVerificationEmail(
        int userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Verifies a user's pending email address using the verification token supplied in the command.
    /// </summary>
    /// <param name="userId">The identifier of the user whose pending email address is being verified.</param>
    /// <param name="command">The command containing the verification token that was sent to the pending email address.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that resolves to a <see cref="VerifyPendingEmailAddressResult"/> indicating whether the
    /// email address was verified, or which <see cref="VerifyPendingEmailAddressError"/> case prevented it
    /// (for example, the user was not found, the caller was not authorised, there was no pending email
    /// address, it was already verified, or the token was invalid).
    /// </returns>
    Task<VerifyPendingEmailAddressResult> VerifyPendingEmailAddress(
        int userId,
        VerifyPendingEmailAddressCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the user's email address to complete a pending email change.
    /// </summary>
    /// <param name="userId">The ID of the user whose email address is being updated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the result of the pending email change.
    /// </returns>
    Task<UpdateEmailToCompletePendingEmailChangeResult> UpdateEmailToCompletePendingEmailChange(
        int userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Cancels the specified user's unverified pending email change.
    /// </summary>
    /// <param name="userId">The identifier of the user whose pending email change is being cancelled.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that resolves to a <see cref="Result{TError}"/> indicating whether the pending email
    /// change was cancelled, or which <see cref="CancelPendingEmailChangeError"/> case prevented it.
    /// </returns>
    Task<Result<CancelPendingEmailChangeError>> CancelPendingEmailChange(
        int userId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves the details of a user, along with their membership of the specified organisation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="organisationId">
    /// The unique identifier of the organisation the user's membership should be read from. A user
    /// may belong to several organisations, so their role is resolved against this organisation.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's
    /// details and their role within the organisation, or the reason the request failed.
    /// </returns>
    Task<Result<UserInformationDto, GetUsersError>> GetUserDetailsWithinOrganisation(
        int userId,
        int organisationId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves the administrator's view of a user's details, along with their membership of the
    /// specified organisation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="organisationId">
    /// The unique identifier of the organisation the user's membership should be read from. A user
    /// may belong to several organisations, so their role is resolved against this organisation.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> with the user's details as seen by an administrator
    /// (see <see cref="UserInformationAdminViewDto"/>) and their role within the organisation, or
    /// an error of type <see cref="GetUsersError"/> if the request failed.
    /// </returns>
    Task<
        Result<UserInformationAdminViewDto, GetUsersError>
    > GetUserDetailsWithinOrganisationAdminView(
        int userId,
        int organisationId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the currently authenticated user's active organisation.
    /// </summary>
    /// <param name="command">The organisation update request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result indicates whether the
    /// current user's organisation was successfully updated or why the update failed.
    /// </returns>
    Task<Result<UpdateCurrentOrganisationError>> UpdateCurrentOrganisation(
        UpdateCurrentOrganisationCommand command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Gets the organisations the currently authenticated user is permitted to select as their
    /// current organisation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the active
    /// organisations for which the current user holds an authorised membership, ordered by name.
    /// </returns>
    Task<IReadOnlyCollection<OrganisationListDto>> GetCurrentUserOrganisations(
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Gets distinct champion work email addresses for Active or Inactive memberships in the current organisation.
    /// The current user's membership may be deactivated; an error is returned when the
    /// organisation context is unavailable or ineligible.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The champion work email addresses, or an error for unavailable or ineligible context.</returns>
    Task<
        Result<IReadOnlyCollection<string>, GetCurrentOrganisationChampionsError>
    > GetCurrentOrganisationChampionEmails(CancellationToken cancellationToken);

    /// <summary>
    /// Removes access and anonymises the user's profile and linked approved registration requests.
    /// Personal details in user-change audit history are cleared. Records created by the user are
    /// kept; other registration requests, communications, logs and backups are not deleted.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> with the name the user now appears under, or an
    /// error of type <see cref="RemoveUserError"/>.
    /// </returns>
    Task<Result<RemovedUserDto, RemoveUserError>> RemoveUser(
        int userId,
        CancellationToken cancellationToken
    );
}
