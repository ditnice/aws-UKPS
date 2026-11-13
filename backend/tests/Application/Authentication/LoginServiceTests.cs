using Amazon.CognitoIdentityProvider.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.Authentication.Dtos;
using UKPS.Api.Application.Authentication.Errors;
using UKPS.Api.Application.Authentication.SecurityNotification;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using LoginResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.InternalServices.Identity.InitiateAuthenticationError
>;

namespace UKPS.Api.Tests.Application.Authentication;

[Collection(DatabaseCollection.Name)]
public class LoginServiceTests : DatabaseTestBase
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly IServiceTestHarness<ILoginService> _harness;

    private readonly DateTime _testTime = new DateTime(2022, 10, 11, 12, 14, 48, DateTimeKind.Utc);
    private LoginRequest _defaultLoginRequest = null!;
    private RespondToMultiFactorAuthenticationChallengeCommand _defaultResponseToMfaCommand = null!;
    private User _user = null!;

    public LoginServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<ILoginService>(Context)
            .UpdateCurrentTime(_testTime)
            .ConfigureServices(services =>
            {
                services.AddSingleton(
                    Options.Create(
                        new CognitoOptions
                        {
                            ClientId = "client-id",
                            Region = "eu-west-2",
                            ServiceUrlOverride = new Uri("https://cognito.example.com"),
                            ClientSecret = "client-secret",
                            UserPoolId = "user-pool-id",
                        }
                    )
                );
                return services;
            });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var testUser = _harness.Cognito.TestUser;
        var userFaker = new UserFaker().RuleFor(
            x => x.CognitoUsername,
            _ => new CognitoUsername() { Value = testUser.Username }
        );
        _user = await AddEntity(userFaker.Generate(), TestContext.Current.CancellationToken);
        _defaultLoginRequest = new LoginRequest()
        {
            Username = _user.WorkEmail,
            Password =
                testUser.Password
                ?? throw new InvalidOperationException("The test users password is not set."),
        };
        _defaultResponseToMfaCommand = new RespondToMultiFactorAuthenticationChallengeCommand()
        {
            Username = _user.WorkEmail,
            Code = _harness.Cognito.ValidMfaCode,
            AuthenticationSession = "authentication-session",
        };
    }

    [Fact]
    public async Task Login_ShouldRespondWithAChallenge()
    {
        LoginResult loginResult = await _harness.Service.Login(
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );
        loginResult.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Challenge>();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenCodeAndSessionAreCorrect_ShouldReturnSuccessfulLogin()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();

        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenCodeIsIncorrect_ShouldReturnError()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();
        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
                Code = "incorrect-code",
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_SessionIsIncorrect_ShouldReturnError()
    {
        InitiateAuthenticationError.Challenge _ = await AttemptLoginAndGetChallenge();
        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = "incorrect-session",
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task RefreshAuthenticationToken_ShouldReturnRefreshedAuthenticationCredentials()
    {
        AuthenticationCredentialsDto credentials = await CompleteFullLogin();
        LoginResult result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = credentials.RefreshToken },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task RefreshAuthenticationToken_WhenRefreshTokenIsInvalid_ShouldReturnUnauthorisedResult()
    {
        AuthenticationCredentialsDto _ = await CompleteFullLogin();
        var result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = "invalid-token" },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task SignOut_ShouldRevokeTheRefreshToken()
    {
        AuthenticationCredentialsDto credentials = await CompleteFullLogin();

        await _harness.Service.SignOut(
            credentials.RefreshToken,
            TestContext.Current.CancellationToken
        );

        var result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = credentials.RefreshToken },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<InitiateAuthenticationError.Unauthorised>();
    }

    [Fact]
    public async Task SignOut_WhenRefreshTokenIsInvalid_ShouldCompleteSuccessfully()
    {
        Exception? exception = await Record.ExceptionAsync(() =>
            _harness.Service.SignOut("invalid-token", TestContext.Current.CancellationToken)
        );

        exception.ShouldBeNull();
    }

    [Fact]
    public async Task SignOut_WhenCognitoIsThrottling_ShouldCompleteSuccessfully()
    {
        _harness
            .Cognito.Mock.RevokeTokenAsync(
                Arg.Any<RevokeTokenRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Throws(new TooManyRequestsException("Rate exceeded"));

        Exception? exception = await Record.ExceptionAsync(() =>
            _harness.Service.SignOut(
                _harness.Cognito.RefreshToken,
                TestContext.Current.CancellationToken
            )
        );

        exception.ShouldBeNull();
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenUserHasOneSelectableOrganisation_ShouldSelectIt()
    {
        UserOrgMembership[] memberships = await AddMemberships(
            UserOrgStatus.Active,
            UserOrgStatus.Inactive
        );

        await CompleteFullLogin();

        (await GetSelectedOrganisationIds()).ShouldBe([memberships[0].OrganisationId]);
    }

    [Fact]
    public async Task RespondToMultiFactorAuthenticationChallenge_WhenUserHasManySelectableOrganisations_ShouldClearTheSelection()
    {
        UserOrgMembership[] memberships = await AddMemberships(
            UserOrgStatus.Active,
            UserOrgStatus.Active
        );
        await SelectOrganisation(memberships[1]);

        await CompleteFullLogin();

        (await GetSelectedOrganisationIds()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Login_WhenAChallengeIsReturned_ShouldNotChangeTheSelection()
    {
        UserOrgMembership[] memberships = await AddMemberships(
            UserOrgStatus.Active,
            UserOrgStatus.Active
        );
        await SelectOrganisation(memberships[1]);

        await AttemptLoginAndGetChallenge();

        (await GetSelectedOrganisationIds()).ShouldBe([memberships[1].OrganisationId]);
    }

    [Fact]
    public async Task RefreshAuthenticationToken_ShouldNotChangeTheSelection()
    {
        UserOrgMembership[] memberships = await AddMemberships(
            UserOrgStatus.Active,
            UserOrgStatus.Active
        );
        AuthenticationCredentialsDto credentials = await CompleteFullLogin();
        await SelectOrganisation(memberships[1]);

        LoginResult result = await _harness.Service.RefreshAuthenticationToken(
            new RefreshAuthenticationTokenCommand() { RefreshToken = credentials.RefreshToken },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess();
        (await GetSelectedOrganisationIds()).ShouldBe([memberships[1].OrganisationId]);
    }

    [Fact]
    public async Task InitiateForgottenPasswordFlow_ShouldReturnSuccessResult()
    {
        await PerformForBothRealAndNonRealUser(
            async (workEmail) =>
            {
                var result = await _harness.Service.InitiateForgottenPasswordProcedure(
                    new() { Email = workEmail },
                    Ct
                );

                result.ShouldBeSuccess();
            }
        );
    }

    [Fact]
    public async Task InitiateForgottenPasswordFlow_WhenTooManyAttemptsHaveOccurred_ShouldReturnTooManyAttemptsError()
    {
        _harness
            .Cognito.Mock.ForgotPasswordAsync(
                Arg.Any<ForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<LimitExceededException>();

        await PerformForBothRealAndNonRealUser(
            async (workEmail) =>
            {
                Result<InitiateForgottenPasswordProcedureError> result = await _harness
                    .UpdateCurrentTime(_testTime)
                    .Service.InitiateForgottenPasswordProcedure(new() { Email = workEmail }, Ct);
                result
                    .ShouldBeError()
                    .ShouldBeOfType<InitiateForgottenPasswordProcedureError.NumberOfAttemptsExceeded>();
            }
        );
    }

    [Fact]
    public async Task InitiateForgottenPasswordFlow_WhenUserExists_ShouldSendForgotPasswordCommandToCognito()
    {
        _ = await _harness.Service.InitiateForgottenPasswordProcedure(
            new() { Email = _user.WorkEmail },
            Ct
        );

        _harness
            .Cognito.ResetPasswordCommands.ShouldHaveSingleItem()
            .Username.ShouldBe(_user.CognitoUsername.Value);
    }

    [Fact]
    public async Task InitiateForgottenPasswordFlow_WhenUserDoesNotExist_ShouldSendForgotPasswordCommandToCognito()
    {
        _ = await _harness.Service.InitiateForgottenPasswordProcedure(
            new() { Email = "notreal@email.com" },
            Ct
        );

        _harness.Cognito.ResetPasswordCommands.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenAResetRequestHasBeenInitiated_ShouldReturnSuccess()
    {
        Result<InitiateForgottenPasswordProcedureError> initiateResult =
            await _harness.Service.InitiateForgottenPasswordProcedure(
                new() { Email = _user.WorkEmail },
                Ct
            );
        initiateResult.ShouldBeSuccess();

        var updatedPassword = "new-password";
        var result = await MakeValidConfirmPasswordRequest(x =>
            x with
            {
                NewPassword = updatedPassword,
            }
        );
        result.ShouldBeSuccess();

        var resetCommand = _harness.Cognito.ResetPasswordCommands.ShouldHaveSingleItem();
        resetCommand.ShouldBe(
            new MockAmazonCognitoIdentityProvider.ResetPasswordCommand()
            {
                Username = _user.CognitoUsername.Value,
                UpdatedPassword = updatedPassword,
            }
        );
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenAResetRequestHasBeenInitiatedAndUserExist_ShouldSendSecurityNotification()
    {
        Result<InitiateForgottenPasswordProcedureError> initiateResult =
            await _harness.Service.InitiateForgottenPasswordProcedure(
                new() { Email = _user.WorkEmail },
                Ct
            );
        initiateResult.ShouldBeSuccess();

        var updatedPassword = "new-password";
        var result = await MakeValidConfirmPasswordRequest(x =>
            x with
            {
                NewPassword = updatedPassword,
            }
        );
        result.ShouldBeSuccess();

        _harness
            .Emails.Sent.ShouldHaveSingleItem()
            .ShouldBeOfType<SecurityNotificationEmail>()
            .ChangedValue.ShouldBe(ChangedUserData.Password);
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenIncorrectConfirmationCodeIsProvided_ShouldReturnCodeMismatchError()
    {
        Result<InitiateForgottenPasswordProcedureError> initiateResult =
            await _harness.Service.InitiateForgottenPasswordProcedure(
                new() { Email = _user.WorkEmail },
                Ct
            );
        initiateResult.ShouldBeSuccess();

        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest(x =>
            x with
            {
                ConfirmationCode = "invalid-code",
            }
        );

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.CodeMismatch>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenInvalidPasswordIsProvided_ShouldReturnInvalidPassword()
    {
        Result<InitiateForgottenPasswordProcedureError> initiateResult =
            await _harness.Service.InitiateForgottenPasswordProcedure(
                new() { Email = _user.WorkEmail },
                Ct
            );
        initiateResult.ShouldBeSuccess();

        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest(x =>
            x with
            {
                NewPassword = _harness.Cognito.InvalidPassword,
            }
        );

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.InvalidPassword>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenCognitoThrowsRequestLimitExceededException_ShouldReturnTooManyRequestsError()
    {
        _harness
            .Cognito.Mock.ConfirmForgotPasswordAsync(
                Arg.Any<ConfirmForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<LimitExceededException>();
        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest();

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.TooManyRequests>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenCognitoThrowsTooManyRequestsExceptionException_ShouldReturnTooManyRequestsError()
    {
        _harness
            .Cognito.Mock.ConfirmForgotPasswordAsync(
                Arg.Any<ConfirmForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<TooManyRequestsException>();
        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest();

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.TooManyRequests>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenCognitoThrowsTooManyFailedAttemptsException_ShouldReturnTooManyFailedAttemptsError()
    {
        _harness
            .Cognito.Mock.ConfirmForgotPasswordAsync(
                Arg.Any<ConfirmForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<TooManyFailedAttemptsException>();
        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest();

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.TooManyFailedAttempts>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenCodeExpires_ShouldReturnCodeExpiredError()
    {
        _harness
            .Cognito.Mock.ConfirmForgotPasswordAsync(
                Arg.Any<ConfirmForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<ExpiredCodeException>();
        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest();

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.CodeExpired>();
    }

    [Fact]
    public async Task ConfirmNewPassword_WhenHistoryPolicyViolated_ShouldReturnHistoryPolicyViolationError()
    {
        _harness
            .Cognito.Mock.ConfirmForgotPasswordAsync(
                Arg.Any<ConfirmForgotPasswordRequest>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync<PasswordHistoryPolicyViolationException>();
        Result<ConfirmNewPasswordError> result = await MakeValidConfirmPasswordRequest();

        result.ShouldBeError().ShouldBeOfType<ConfirmNewPasswordError.HistoryPolicyViolation>();
    }

    private Task<Result<ConfirmNewPasswordError>> MakeValidConfirmPasswordRequest(
        Func<ConfirmNewPasswordCommand, ConfirmNewPasswordCommand>? overrides = null
    )
    {
        var defaultCommand = new ConfirmNewPasswordCommand()
        {
            Email = _user.WorkEmail,
            ConfirmationCode = _harness.Cognito.ValidForgotPasswordCode,
            NewPassword = "new-password",
        };
        var command = overrides is null ? defaultCommand : overrides(defaultCommand);
        return _harness.Service.ConfirmNewPassword(command, Ct);
    }

    private async Task PerformForBothRealAndNonRealUser(Func<string, Task> task)
    {
        string[] cases = [_user.WorkEmail, "non-existent-user@email.com"];

        foreach (var workEmail in cases)
        {
            await task(workEmail);
        }
    }

    private async Task<UserOrgMembership[]> AddMemberships(
        params UserOrgStatus[] organisationStatuses
    )
    {
        UserOrgMembership[] memberships = organisationStatuses
            .Select(status =>
                new UserOrgMembershipFaker()
                    .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active)
                    .Generate()
                    .Update(x =>
                    {
                        x.UserId = _user.Id;
                        x.Organisation = new OrganisationFaker()
                            .Generate()
                            .Update(o => o.Status = status);
                    })
            )
            .ToArray();

        await AddEntities(memberships, TestContext.Current.CancellationToken);
        return memberships;
    }

    private async Task SelectOrganisation(UserOrgMembership membership)
    {
        await Context
            .UserOrgMemberships.Where(x => x.Id == membership.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(m => m.IsSelectedAsCurrentOrganisation, true),
                TestContext.Current.CancellationToken
            );
    }

    private Task<int[]> GetSelectedOrganisationIds() =>
        _harness
            .GetClearedContext()
            .UserOrgMemberships.Where(x =>
                x.UserId == _user.Id && x.IsSelectedAsCurrentOrganisation
            )
            .Select(x => x.OrganisationId)
            .ToArrayAsync(TestContext.Current.CancellationToken);

    private async Task<InitiateAuthenticationError.Challenge> AttemptLoginAndGetChallenge()
    {
        LoginResult loginResult = await _harness.Service.Login(
            _defaultLoginRequest,
            TestContext.Current.CancellationToken
        );
        InitiateAuthenticationError.Challenge challenge = loginResult
            .ShouldBeError()
            .ShouldBeOfType<InitiateAuthenticationError.Challenge>();
        return challenge;
    }

    private async Task<AuthenticationCredentialsDto> CompleteFullLogin()
    {
        InitiateAuthenticationError.Challenge challenge = await AttemptLoginAndGetChallenge();

        LoginResult result = await _harness.Service.RespondToMultiFactorAuthenticationChallenge(
            _defaultResponseToMfaCommand with
            {
                AuthenticationSession = challenge.AuthenticationSession,
            },
            TestContext.Current.CancellationToken
        );
        AuthenticationCredentialsDto loginResult = result.ShouldBeSuccess();
        return loginResult;
    }
}
