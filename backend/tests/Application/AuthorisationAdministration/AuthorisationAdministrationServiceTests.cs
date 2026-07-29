using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using UKPS.Api.Application.AuthorisationAdministration;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;
using UserSetupResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.UserSetupError>;

namespace UKPS.Api.Tests.Application.AuthorisationAdministration;

[Collection(DatabaseCollection.Name)]
public class AuthorisationAdministrationServiceTests : DatabaseTestBase
{
    private readonly Faker<UserOnboardingRecord> _userOnboardingRecordFaker;
    private readonly IServiceTestHarness<IAuthorisationAdministrationService> _harness;
    private readonly DateTime _testTime = new DateTime(2022, 10, 11, 12, 14, 48, DateTimeKind.Utc);
    private readonly string _currentUser = "test.user@email.com";
    private TimeSpan _testExpiryTokenTime = TimeSpan.FromMinutes(15);
    private readonly SetupUserCommand _validSetupUserCommand = new()
    {
        SetupToken = Guid.CreateVersion7(),
        NewPassword = "9U26=e6p9g[R",
    };

    public AuthorisationAdministrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _userOnboardingRecordFaker = new UserOnboardingRecordFaker().RuleFor(
            x => x.NewUserOrganisation,
            _ => new OrganisationFaker().Generate()
        );
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
        DateTime createdAtTime = _testTime - TimeSpan.FromMinutes(minutesInThePast);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        await AddEntity(entity, TestContext.Current.CancellationToken);

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
        DateTime createdAtTime = _testTime + TimeSpan.FromMinutes(10);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        await AddEntity(entity, TestContext.Current.CancellationToken);

        Func<Task<SetupTokenValidationResult>> act = () =>
            _harness.Service.Validate(entity.SetupToken, TestContext.Current.CancellationToken);

        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Validate_WhenReferencingASetupTokenThatHasAlreadyBeenConsumed_ReturnsConsumedError()
    {
        DateTime createdAtTime = _testTime - TimeSpan.FromMinutes(15);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        entity.MarkAsConsumed(_testTime);
        await AddEntity(entity, TestContext.Current.CancellationToken);

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
        DateTime createdAtTime = _testTime - TimeSpan.FromMinutes(minutesInThePast);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        await AddEntity(entity, TestContext.Current.CancellationToken);

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
        DateTime createdAtTime = _testTime + TimeSpan.FromMinutes(10);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        await AddEntity(entity, TestContext.Current.CancellationToken);

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
        DateTime createdAtTime = _testTime - TimeSpan.FromMinutes(15);
        UserOnboardingRecord entity = _userOnboardingRecordFaker
            .RuleFor(x => x.CreatedAt, _ => createdAtTime)
            .Generate();
        entity.MarkAsConsumed(_testTime);
        await AddEntity(entity, TestContext.Current.CancellationToken);

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

    private IServiceTestHarness<IAuthorisationAdministrationService> CreateTestHarness()
    {
        return new ServiceTestHarness<IAuthorisationAdministrationService>(Context)
            .UpdateCurrentTime(_testTime)
            .UpdateCurrentUser(x => x with { Email = _currentUser })
            .ConfigureServices(services =>
            {
                services
                    .AddOptions<UserOnboardingConfiguration>()
                    .Configure(options =>
                        options.SetupTokenExpiryTimeSeconds = (int)_testExpiryTokenTime.TotalSeconds
                    );
                return services;
            });
    }
}
