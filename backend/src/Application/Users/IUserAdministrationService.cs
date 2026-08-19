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
    /// <param name="registerUserDto">
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
    Task<Result<RegisterUserDetailsDto, RegisterUserError>> RegisterUser(
        RegisterUserDto registerUserDto,
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
    Task<Result<OnboardUserError>> OnboardUser(
        OnboardUserCommandDto command,
        CancellationToken cancellationToken
    );
}
