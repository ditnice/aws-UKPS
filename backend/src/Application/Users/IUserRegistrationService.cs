global using GetUserRegistrationByIdResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.RegisterUserConfirmationDto,
    UKPS.Api.Application.Users.Errors.GetUserDetailsError
>;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Provides operations for managing membership requests.
/// </summary>
public interface IUserRegistrationService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="registerUserCommandDto">
    /// The details required to register the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Result{TSuccess, TError}"/> with the registered user's details, or an
    /// error of type <see cref="RegisterUserError"/> if the user could not be registered.
    /// </returns>
    Task<Result<RegisterUserConfirmationDto, RegisterUserError>> RegisterUser(
        int organisationId,
        RegisterUserCommandDto registerUserCommandDto,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves the details of a user.
    /// </summary>
    /// <param name="organisationId">
    /// The identifier of the organisation containing the membership request.
    /// </param>
    /// <param name="id">
    /// The unique identifier of the user whose details are being retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A result containing the user's details if the operation was successful,
    /// or the reason the operation failed.
    /// </returns>
    Task<Result<RegisterUserConfirmationDto, GetUserDetailsError>> GetUserRegistrationById(
        int organisationId,
        int id,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Approves a membership request for the specified user and organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The unique identifier of the organisation associated with the membership request.
    /// </param>
    /// <param name="registrationRequestId">
    /// The unique identifier of the the membership request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an
    /// <see cref="ApproveRequestError"/> if the request could not be approved.
    /// </returns>
    Task<Result<ApproveRequestError>> ApproveRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Rejects a membership request for the specified user and organisation.
    /// </summary>
    /// <param name="organisationId">
    /// The unique identifier of the organisation associated with the membership request.
    /// </param>
    /// <param name="registrationRequestId">
    /// The unique identifier of the the membership request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a
    /// <see cref="RejectRequestError"/> if the request could not be rejected.
    /// </returns>
    Task<Result<RejectRequestError>> RejectRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    );
}
