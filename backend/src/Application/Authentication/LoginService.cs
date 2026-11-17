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
        var user = await GetUserWithMemberships(request.Username, cancellationToken);
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        var result = await _identityService.InitiateAuthentication(
            user.CognitoUsername,
            request.Password,
            cancellationToken
        );

        return await HandleSuccessfulAuthentication(result, user, cancellationToken);
    }

    private async Task<InitiateAuthenticationResult> HandleSuccessfulAuthentication(
        InitiateAuthenticationResult result,
        User user,
        CancellationToken cancellationToken
    )
    {
        if (result.IsErr)
        {
            return result;
        }

        var emailUpdateResult = await _userService.UpdateEmailToCompletePendingEmailChange(
            user.Id,
            cancellationToken
        );

        if (emailUpdateResult.IsErr)
        {
            emailUpdateResult.Error.Match(
                _ => throw new InvalidOperationException(),
                _ => new InitiateAuthenticationError.InvalidPendingEmail()
            );
        }

        await ResetCurrentOrganisation(user, cancellationToken);

        return result;
    }

    public async Task<InitiateAuthenticationResult> RespondToMultiFactorAuthenticationChallenge(
        RespondToMultiFactorAuthenticationChallengeCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await GetUserWithMemberships(command.Username, cancellationToken);
        if (user is null)
        {
            return InitiateAuthenticationResult.Err(new InitiateAuthenticationError.Unauthorised());
        }
        var result = await _identityService.RespondToMultiFactorAuthenticationChallenge(
            user.CognitoUsername,
            command.AuthenticationSession,
            command.Code,
            cancellationToken
        );

        return await HandleSuccessfulAuthentication(result, user, cancellationToken);
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

    private static CognitoUsername CreateDummyUsername(string email)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(email);
        byte[] hash = SHA256.HashData(bytes);
        var hashString = Convert.ToHexString(hash).Substring(0, 32);

        return CognitoUsername.Parse(hashString);
    }

    private Task<User?> GetUserWithMemberships(string email, CancellationToken cancellationToken) =>
        _appDbContext
            .Users.Include(x => x.UserOrgMemberships)!
                .ThenInclude(x => x.Organisation)
            .GetByEmailOrDefault(email, cancellationToken);

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
}
