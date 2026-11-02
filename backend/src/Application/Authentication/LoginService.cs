using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
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
    private readonly AppDbContext _appDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPasswordResetLinkCreator _passwordResetLinkCreator;
    private readonly IEmailService _emailService;
    private readonly AuthenticationOptions _authenticationOptions;

    public LoginService(
        IIdentityService identityService,
        AppDbContext appDbContext,
        IDateTimeProvider dateTimeProvider,
        IPasswordResetLinkCreator passwordRestLinkCreator,
        IEmailService emailService,
        IOptions<AuthenticationOptions> authenticationOptions
    )
    {
        _identityService = identityService;
        _appDbContext = appDbContext;
        _dateTimeProvider = dateTimeProvider;
        _passwordResetLinkCreator = passwordRestLinkCreator;
        _emailService = emailService;
        _authenticationOptions = authenticationOptions.Value;
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
        if (result.IsOk)
        {
            await ResetCurrentOrganisation(user, cancellationToken);
        }
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
        if (result.IsOk)
        {
            await ResetCurrentOrganisation(user, cancellationToken);
        }
        return result;
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
        DateTime dateTime24HoursAgo = _dateTimeProvider.GetUtcNow().AddHours(-24);
        int numberOfPreviousRequestsInTheLast24Hours =
            await _appDbContext.UserPasswordResetRequests.CountAsync(
                x =>
                    x.TargetEmailAddress == EmailHelpers.Normalise(command.Email)
                    && x.CreatedAt > dateTime24HoursAgo,
                cancellationToken
            );

        if (
            numberOfPreviousRequestsInTheLast24Hours
            >= _authenticationOptions.MaxNumberOfChangePasswordAttempts
        )
        {
            return InitiateForgottenPasswordProcedureResult.Err(
                new InitiateForgottenPasswordProcedureError.NumberOfAttemptsExceeded()
            );
        }
        var (userPasswordResetRequest, token) = UserPasswordResetRequest.Create(
            command.Email,
            _dateTimeProvider.GetUtcNow()
        );

        _appDbContext.Add(userPasswordResetRequest);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        var user = await _appDbContext.Users.GetByEmailOrDefault(command.Email, cancellationToken);
        if (user is not null)
        {
            await SendPasswordResetEmail(token, user, cancellationToken);
        }

        return InitiateForgottenPasswordProcedureResult.Ok(); // Return Ok even if the account does not exist to avoid  account enumeration.
    }

    private async Task SendPasswordResetEmail(
        string token,
        User user,
        CancellationToken cancellationToken
    )
    {
        var passwordResetEmail = new PasswordResetEmail()
        {
            Link = _passwordResetLinkCreator.GetPasswordResetLink(token),
        };
        await _emailService.SendEmail(
            new SendEmailCommand()
            {
                RecipientAddress = user.WorkEmail,
                Email = passwordResetEmail,
                PersonIdentifier = user.CognitoUsername,
            },
            cancellationToken
        );
    }

    public Task SignOut(string refreshToken, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(refreshToken)
            ? Task.CompletedTask
            : _identityService.RevokeRefreshToken(refreshToken, cancellationToken);
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
