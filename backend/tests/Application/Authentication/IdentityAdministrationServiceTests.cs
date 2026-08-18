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
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Users;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Authentication.Errors.SetupTokenValidationError>;
using UserSetupResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.MultiFactorAuthenticationSetupDto,
    UKPS.Api.Application.Authentication.Errors.UserSetupError
>;
using VerifyMultiFactorAuthenticationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Authentication.Dtos.AuthenticationCredentialsDto,
    UKPS.Api.Application.Authentication.Errors.VerifyMultiFactorAuthenticationError
>;

namespace UKPS.Api.Tests.Application.Authentication;

[Collection(DatabaseCollection.Name)]
public class IdentityAdministrationServiceTests : DatabaseTestBase
{
    private readonly IServiceTestHarness<IIdentityAdministrationService> _harness;
    private readonly DateTime _testTime = new DateTime(2022, 10, 11, 12, 14, 48, DateTimeKind.Utc);
    private readonly string _currentUser = "test.user@email.com";

    private TimeSpan _testExpiryTokenTime = TimeSpan.FromMinutes(15);
    private readonly SetupUserCommand _validSetupUserCommand = new()
    {
        SetupToken = Guid.CreateVersion7(),
        NewPassword = "9U26=e6p9g[R",
    };

    public IdentityAdministrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = CreateTestHarness();
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(14, true)]
    [InlineData(16, false)]
    [InlineData(30, false)]
    public async Task Validate_WhenSetupTokenAgeExceedsExpirationLimit_ReturnsExpiredError(
        int minutesInThePast,
        bool expectToPassValidation
    )
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: minutesInThePast
        );

        SetupTokenValidationResult result = await _harness.Service.Validate(
            entity.SetupToken,
            TestContext.Current.CancellationToken
        );

        if (expectToPassValidation)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result.ShouldBeError().ShouldBeOfType<SetupTokenValidationError.Expired>();
        }
    }

    [Fact]
    public async Task Validate_WhenNoUserOnboardingRecordExistsInTheDatabase_ReturnsDoesNotExistError()
    {
        Guid noneExistentToken = Guid.CreateVersion7();
        SetupTokenValidationResult result = await _harness.Service.Validate(
            noneExistentToken,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<SetupTokenValidationError.DoesNotExist>();
    }

    [Fact]
    public async Task Validate_WhenReferencingASetupTokenThatWasCreatedInTheFuture_ShouldThrowArgumentException()
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: -10
        );

        Func<Task<SetupTokenValidationResult>> act = () =>
            _harness.Service.Validate(entity.SetupToken, TestContext.Current.CancellationToken);

        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Validate_WhenReferencingASetupTokenThatHasAlreadyBeenConsumed_ReturnsConsumedError()
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: 15,
            consumedMinutesInThePast: 0
        );

        var futureHarness = _harness.UpdateCurrentTime(_testTime + TimeSpan.FromMinutes(5));

        SetupTokenValidationResult validationResult = await futureHarness.Service.Validate(
            entity.SetupToken,
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<SetupTokenValidationError.Consumed>();
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(14, true)]
    [InlineData(16, false)]
    [InlineData(30, false)]
    public async Task SetupUser_WhenSetupTokenAgeExceedsExpirationLimit_ReturnsExpiredError(
        int minutesInThePast,
        bool expectToPassValidation
    )
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: minutesInThePast
        );

        UserSetupResult result = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
            },
            TestContext.Current.CancellationToken
        );

        if (expectToPassValidation)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result.ShouldBeError().ShouldBeOfType<UserSetupError.Expired>();
        }
    }

    [Fact]
    public async Task SetupUser_WhenCognitoThrowsAnNotAuthorizedException_ShouldReturnNotAuthorisedResult()
    {
        _harness
            .Cognito.Mock.AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Throws(new NotAuthorizedException());

        UserOnboardingRecord entity = await CreateUserOnboardingRecord(createdMinutesInThePast: 10);

        UserSetupResult validationResult = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
            },
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<UserSetupError.Unauthorised>();
    }

    [Fact]
    public async Task SetupUser_WhenCognitoReturnsANullAuthenticationResult_ShouldReturnNotAuthorisedResult()
    {
        _harness
            .Cognito.Mock.AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new AdminInitiateAuthResponse());

        UserOnboardingRecord entity = await CreateUserOnboardingRecord(createdMinutesInThePast: 10);

        UserSetupResult validationResult = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
            },
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<UserSetupError.Unauthorised>();
    }

    [Fact]
    public async Task SetupUser_WhenCognitoReturnsANullResponse_ShouldReturnNotAuthorisedResult()
    {
        _harness
            .Cognito.Mock.AdminInitiateAuthAsync(
                Arg.Any<AdminInitiateAuthRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((AdminInitiateAuthResponse)null!);

        UserOnboardingRecord entity = await CreateUserOnboardingRecord(createdMinutesInThePast: 10);

        UserSetupResult validationResult = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
            },
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<UserSetupError.Unauthorised>();
    }

    [Fact]
    public async Task SetupUser_WhenProvidingInvalidPassword_ShouldReturnInvalidPasswordError()
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(createdMinutesInThePast: 15);

        UserSetupResult validationResult = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
                NewPassword = _harness.Cognito.InvalidPassword,
            },
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<UserSetupError.InvalidPassword>();
    }

    [Fact]
    public async Task SetupUser_WhenNoUserOnboardingRecordExistsInTheDatabase_ReturnsDoesNotExistError()
    {
        Guid noneExistentToken = Guid.CreateVersion7();
        UserSetupResult result = await _harness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = noneExistentToken,
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UserSetupError.DoesNotExist>();
    }

    [Fact]
    public async Task SetupUser_WhenReferencingASetupTokenThatWasCreatedInTheFuture_ShouldThrowArgumentException()
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: -10
        );

        Func<Task<UserSetupResult>> act = () =>
            _harness.Service.SetupUser(
                _validSetupUserCommand with
                {
                    SetupToken = entity.SetupToken,
                },
                TestContext.Current.CancellationToken
            );

        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SetupUser_WhenReferencingASetupTokenThatHasAlreadyBeenConsumed_ReturnsConsumedError()
    {
        UserOnboardingRecord entity = await CreateUserOnboardingRecord(
            createdMinutesInThePast: 15,
            consumedMinutesInThePast: 0
        );

        var futureHarness = _harness.UpdateCurrentTime(_testTime + TimeSpan.FromMinutes(5));

        UserSetupResult validationResult = await futureHarness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = entity.SetupToken,
            },
            TestContext.Current.CancellationToken
        );
        validationResult.ShouldBeError().ShouldBeOfType<UserSetupError.Consumed>();
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_ShouldSetupMfa()
    {
        var (setupToken, session, targetUser) = await CreateAndDoInitialUserSetup();

        var result = await _harness.Service.VerifyMultiFactorAuthentication(
            new VerifyMultiFactorAuthenticationCommand()
            {
                SetupToken = setupToken,
                Code = _harness.Cognito.ValidMfaCode,
                AuthenticationSession = session,
            },
            TestContext.Current.CancellationToken
        );
        AuthenticationCredentialsDto credentials = result.ShouldBeSuccess();
        credentials.AccessToken.ShouldBe("access-token");
        credentials.RefreshToken.ShouldBe(_harness.Cognito.RefreshToken);

        _harness.Cognito.GetUser(targetUser).ShouldNotBeNull().MfaSetup.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_ShouldMarkMembershipAsActive()
    {
        var (setupToken, session, userIdentityId) = await CreateAndDoInitialUserSetup();

        var result = await _harness.Service.VerifyMultiFactorAuthentication(
            new VerifyMultiFactorAuthenticationCommand()
            {
                SetupToken = setupToken,
                Code = _harness.Cognito.ValidMfaCode,
                AuthenticationSession = session,
            },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        var user = await _harness
            .GetClearedContext()
            .Users.Include(x => x.UserOrgMemberships)
            .SingleOrDefaultAsync(
                x => x.CognitoUsername == userIdentityId,
                TestContext.Current.CancellationToken
            );

        user.ShouldNotBeNull();
        user.UserOrgMemberships!.ShouldAllBe(x => x.Status == UserOrgStatus.Active);
    }

    [Fact]
    public async Task VerifyMultiFactorAuthentication_WhenSuppliedIncorrectCode_ShouldNotSetupMfaAndReturnInvalidCodeError()
    {
        var (setupToken, session, userIdentityId) = await CreateAndDoInitialUserSetup();

        VerifyMultiFactorAuthenticationResult result =
            await _harness.Service.VerifyMultiFactorAuthentication(
                new VerifyMultiFactorAuthenticationCommand()
                {
                    SetupToken = setupToken,
                    Code = "9999",
                    AuthenticationSession = session,
                },
                TestContext.Current.CancellationToken
            );
        result.ShouldBeError().ShouldBeOfType<VerifyMultiFactorAuthenticationError.InvalidCode>();

        _harness.Cognito.GetUser(userIdentityId).ShouldNotBeNull().MfaSetup.ShouldBeFalse();
    }

    private async Task<(
        Guid SetupToken,
        string Session,
        UserIdentityId userIdentityId
    )> CreateAndDoInitialUserSetup()
    {
        UserOnboardingRecord onboardingRecord = await OnboardUserWithTimeOffset(minutesInPast: 15);
        MultiFactorAuthenticationSetupDto setupResult = await SetupUserWithTimeOffset(
            onboardingRecord.SetupToken,
            0
        );
        return (
            onboardingRecord.SetupToken,
            setupResult.AuthenticationSession,
            onboardingRecord.User!.CognitoUsername
        );
    }

    private async Task<UserOnboardingRecord> OnboardUserWithTimeOffset(int minutesInPast)
    {
        var organisationFaker = new OrganisationFaker();
        Organisation organisation = await AddEntity(
            organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        DateTime createdAtTime = _testTime - TimeSpan.FromMinutes(minutesInPast);
        var userAdminHarness = new ServiceTestHarness<IUserAdministrationService>(
            _harness
        ).UpdateCurrentTime(createdAtTime);
        OnboardUserCommandDtoFaker onboardUserCommandDtoFaker = new OnboardUserCommandDtoFaker();
        Result<int, OnboardUserError> result = await userAdminHarness.Service.OnboardUser(
            onboardUserCommandDtoFaker.Generate() with
            {
                OrganisationId = organisation.Id,
            },
            TestContext.Current.CancellationToken
        );
        var userId = result.ShouldBeSuccess();

        var user =
            await userAdminHarness
                .GetClearedContext()
                .Users.Include(x => x.OnboardingRecord)
                .FirstOrDefaultAsync(x => x.Id == userId, TestContext.Current.CancellationToken)
            ?? throw new InvalidOperationException("Could not find expected user.");

        return user.OnboardingRecord!;
    }

    private async Task<MultiFactorAuthenticationSetupDto> SetupUserWithTimeOffset(
        Guid setupToken,
        int minutesInPast
    )
    {
        DateTime consumedAtTime = _testTime - TimeSpan.FromMinutes(minutesInPast);
        var setupUserHarness = ConfigureTestHarness(
                new ServiceTestHarness<IIdentityAdministrationService>(_harness)
            )
            .UpdateCurrentTime(consumedAtTime);
        var result = await setupUserHarness.Service.SetupUser(
            _validSetupUserCommand with
            {
                SetupToken = setupToken,
            },
            TestContext.Current.CancellationToken
        );
        return result.ShouldBeSuccess();
    }

    private async Task<UserOnboardingRecord> CreateUserOnboardingRecord(
        int createdMinutesInThePast,
        int? consumedMinutesInThePast = null
    )
    {
        var onboardingRecord = await OnboardUserWithTimeOffset(createdMinutesInThePast);
        if (consumedMinutesInThePast is { } value)
        {
            await SetupUserWithTimeOffset(onboardingRecord.SetupToken, value);
        }

        return onboardingRecord;
    }

    private IServiceTestHarness<IIdentityAdministrationService> CreateTestHarness()
    {
        return ConfigureTestHarness(
            new ServiceTestHarness<IIdentityAdministrationService>(Context)
        );
    }

    private IServiceTestHarness<IIdentityAdministrationService> ConfigureTestHarness(
        ServiceTestHarness<IIdentityAdministrationService> harness
    )
    {
        return harness
            .UpdateCurrentTime(_testTime)
            .UpdateCurrentUser(x => x with { Email = _currentUser })
            .ConfigureServices(services =>
            {
                services
                    .AddOptions<UserOnboardingConfiguration>()
                    .Configure(options =>
                        options.SetupTokenExpiryTimeSeconds = (int)_testExpiryTokenTime.TotalSeconds
                    );
                services.AddSingleton(
                    Options.Create(
                        new CognitoConfiguration
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
}
