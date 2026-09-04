using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;

namespace UKPS.Api.Application.Users;

/// <summary>
/// Provides operations for managing user administration tasks.
/// </summary>
public interface IUserAdministrationService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
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
        RegisterUserCommandDto registerUserCommandDto,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Onboards a new user into the system.
    /// </summary>
    /// <param name="command">
    /// The command containing the details of the user to onboard.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A result indicating whether the user was successfully onboarded or
    /// containing the reason the operation failed.
    /// </returns>
    Task<Result<int, OnboardUserError>> OnboardUser(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves the details of a user.
    /// </summary>
    /// <param name="Id">
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
        int Id,
        CancellationToken cancellationToken
    );
}
