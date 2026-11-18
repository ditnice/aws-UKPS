using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Authentication.SecurityNotification;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.Users;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using InitiateAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Application.Authentication;

internal class LoginService : ILoginService
{
    private readonly IIdentityService _identityService;
    private readonly IUserService _userService;
    private readonly AppDbContext _appDbContext;
    private readonly IEmailService _emailService;

    public LoginService(
        IIdentityService identityService,
        IUserService userService,
        AppDbContext appDbContext,
        IEmailService emailService
    )
    {
        _identityService = identityService;
        _userService = userService;
        _appDbContext = appDbContext;
        _emailService = emailService;
    }

    public async Task<InitiateAuthenticationResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        UserLookupResult? lookupResult = await RetrieveUserToLogin(
            request.Username,
            cancellationToken
        );

        if (lookupResult is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }

        var result = await _identityService.InitiateAuthentication(
            lookupResult.User.CognitoUsername,
            request.Password,
            cancellationToken
        );

        return await HandlePostAuthentication(result, lookupResult, cancellationToken);
    }

    public async Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        UserLookupResult? lookupResult = await RetrieveUserToLogin(
            command.Username,
            cancellationToken
        );
        if (lookupResult is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        var result = await _identityService.RespondToMultiFactorAuthenticationChallenge(
            lookupResult.User.CognitoUsername,
            command.AuthenticationSession,
            command.Code,
            cancellationToken
        );

        return await HandlePostAuthentication(result, lookupResult, cancellationToken);
    }

    public Task<InitiateAuthenticationResult> RefreshAuthenticationToken(
        RefreshAuthenticationTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        return _identityService.RefreshAuthenticationToken(command.RefreshToken, cancellationToken);
    }

    public async Task<InitiateForgottenPasswordProcedureResult> InitiateForgottenPasswordProcedure(
        InitiateForgottenPasswordProcedureCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await _appDbContext.Users.GetByEmailOrDefault(command.Email, cancellationToken);

        var username = user is not null ? user.CognitoUsername : CreateDummyUsername(command.Email);

        var success = await _identityService.SendForgottenPasswordCommand(
            username,
            cancellationToken
        );
        if (!success)
        {
            return InitiateForgottenPasswordProcedureResult.Err(
                new InitiateForgottenPasswordProcedureError.NumberOfAttemptsExceeded()
            );
        }

        return InitiateForgottenPasswordProcedureResult.Ok(); // Return Ok even if the account does not exist to avoid  account enumeration.
    }

    public async Task<ConfirmNewPasswordResult> ConfirmNewPassword(
        ConfirmNewPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await _appDbContext.Users.GetByEmailOrDefault(command.Email, cancellationToken);
        var username = user is not null ? user.CognitoUsername : CreateDummyUsername(command.Email);
        var result = await _identityService.ConfirmNewPassword(
            username,
            command.ConfirmationCode,
            command.NewPassword,
            cancellationToken
        );

        if (user is null || !result.IsOk)
            return result;

        await _emailService.SendEmail(
            new SendEmailCommand()
            {
                RecipientAddress = user.WorkEmail,
                PersonIdentifier = user.CognitoUsername,
                Email = new SecurityNotificationEmail() { ChangedValue = ChangedUserData.Password },
            },
            cancellationToken
        );

        return result;
    }

    public Task SignOut(string refreshToken, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(refreshToken)
            ? Task.CompletedTask
            : _identityService.RevokeRefreshToken(refreshToken, cancellationToken);
    }

    private async Task<InitiateAuthenticationResult> HandlePostAuthentication(
        InitiateAuthenticationResult result,
        UserLookupResult userLookupResult,
        CancellationToken cancellationToken
    )
    {
        if (result.IsErr)
        {
            return result;
        }

        if (userLookupResult.FoundFromPendingEmail)
        {
            var emailUpdateResult = await _userService.UpdateEmailToCompletePendingEmailChange(
                userLookupResult.User.Id,
                cancellationToken
            );

            if (emailUpdateResult.IsErr)
            {
                var error = emailUpdateResult.Error.Match<InitiateAuthenticationError>(
                    notFound: _ =>
                        throw new InvalidOperationException(
                            "Failed to update pending email address. User was not found despite previously being found during this request."
                        ),
                    conflictingEmail: _ =>
                        throw new InvalidOperationException(
                            "Failed to update pending email address. Conflicting email occured despite checks to ensure email/pending email uniqueness"
                        ),
                    notVerified: _ => new InitiateAuthenticationError.VerificationRequired()
                );
                return InitiateAuthenticationResult.Err(error);
            }
        }

        await ResetCurrentOrganisation(userLookupResult.User, cancellationToken);

        return result;
    }

    private async Task<UserLookupResult?> RetrieveUserToLogin(
        string emailUsername,
        CancellationToken cancellationToken
    )
    {
        static IQueryable<User> ApplyRequiredInclusions(IQueryable<User> users)
        {
            return users.Include(x => x.UserOrgMemberships)!.ThenInclude(x => x.Organisation);
        }

        var user = await ApplyRequiredInclusions(_appDbContext.Users)
            .GetByEmailOrDefault(emailUsername, cancellationToken);
        if (user is not null)
        {
            return new(user, false);
        }
        var userWithPendingEmail = await ApplyRequiredInclusions(_appDbContext.Users)
            .GetByPendingEmailOrDefault(emailUsername, cancellationToken);
        return userWithPendingEmail is not null ? new(userWithPendingEmail, true) : null;
    }

    private static CognitoUsername CreateDummyUsername(string email)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(email);
        byte[] hash = SHA256.HashData(bytes);
        var hashString = Convert.ToHexString(hash).Substring(0, 32);

        return CognitoUsername.Parse(hashString);
    }

    // Each sign-in clears the selected organisation, then selects it again only when there is
    // just one to choose. If the flag is left null (for users with multiple memberships), they
    // are routed to the organisation picker page.
    private async Task ResetCurrentOrganisation(User user, CancellationToken cancellationToken)
    {
        user.ResetCurrentOrganisation();
        await _appDbContext.SaveChangesAsync(cancellationToken);

        int? soleOrganisationId = user.FindSoleSelectableOrganisationId();
        if (soleOrganisationId.HasValue)
        {
            user.TryUpdateCurrentOrganisation(soleOrganisationId.Value);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed record UserLookupResult(User User, bool FoundFromPendingEmail);
}
