using Bogus;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using OnBoardUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.OnboardUserError>;

namespace UKPS.Api.Tests.Application.Users;

[Collection(DatabaseCollection.Name)]
public class UserAdministrationServiceTests : DatabaseTestBase
{
    private readonly IServiceTestHarness<IUserAdministrationService> _harness;
    private readonly DateTime _currentTime = new DateTime(
        2022,
        10,
        10,
        12,
        56,
        20,
        DateTimeKind.Utc
    );
    private readonly string _currentUserEmail = "current.user@email.com";
    private readonly ISetupLinkCreator _setupLinkCreator = Substitute.For<ISetupLinkCreator>();

    public UserAdministrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = GetTestHarness();
    }

    [Fact]
    public async Task OnboardUser_ShouldCreateANewOnboardingRecordInTheDatabase()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnBoardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        var foundUserRecord = _harness.Context.UserOnboardingRecords.SingleOrDefault(x =>
            x.UserEmail == command.NewUserEmail
        );

        foundUserRecord.ShouldNotBeNull();
        foundUserRecord.UserEmail.ShouldBe(command.NewUserEmail);
        foundUserRecord.CreatedAt.ShouldBe(_currentTime);
        foundUserRecord.CreatedBy.ShouldBe(_currentUserEmail);
    }

    [Fact]
    public async Task OnboardUser_ShouldSendAUserSignUpRequestEmailIncludingALinkGeneratedFromTheSetupLinkCreator()
    {
        var testLink = "test link";
        _setupLinkCreator.GetSetupLink(Arg.Any<Guid>()).Returns(testLink);

        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnBoardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        UserSignUpRequestEmail email = _harness
            .Emails.Sent.ShouldHaveSingleItem()
            .ShouldBeOfType<UserSignUpRequestEmail>();

        email.Link.ShouldBe(testLink);
    }

    [Fact]
    public async Task OnBoardUser_ForOtherOtherOrganisations_ShouldReturnNotAllowedResultUnlessASuperUser()
    {
        IEnumerable<UserRole> noneSuperAdminRoles = Enum.GetValues<UserRole>()
            .Except([UserRole.Super]);

        foreach (var userRole in noneSuperAdminRoles)
        {
            IServiceTestHarness<IUserAdministrationService> harnessWithNoneSuperUserAuth =
                GetTestHarness().UpdateCurrentUser(x => x with { UserRole = userRole });
            OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
            OnBoardUserResult result = await harnessWithNoneSuperUserAuth.Service.OnboardUser(
                command,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeError().ShouldBeOfType<OnboardUserError.NotAllowed>();
        }
    }

    [Theory]
    [InlineData(UserRole.Super, true)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Standard, false)]
    public async Task OnBoardUser_ForSameOrganisation_ShouldReturnNotAllowedResultUnlessASuperUserOrChampion(
        UserRole userRole,
        bool allowed
    )
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        IServiceTestHarness<IUserAdministrationService> harnessWithNoneSuperUserAuth =
            GetTestHarness()
                .UpdateCurrentUser(x =>
                    x with
                    {
                        UserRole = userRole,
                        OrganisationId = command.OrganisationId,
                    }
                );
        OnBoardUserResult result = await harnessWithNoneSuperUserAuth.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );

        if (allowed)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result.ShouldBeError().ShouldBeOfType<OnboardUserError.NotAllowed>();
        }
    }

    [Fact]
    public async Task OnBoardUser_ShouldReturnInvalidOrganisationErrorIfReferencingOrganisationThatDoesNotExist()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand() with
        {
            OrganisationId = 999,
        };
        OnBoardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<OnboardUserError.InvalidOrganisation>();
    }

    private IServiceTestHarness<IUserAdministrationService> GetTestHarness()
    {
        return new ServiceTestHarness<IUserAdministrationService>(Context)
            .UpdateCurrentUser(x => x with { Email = _currentUserEmail })
            .UpdateCurrentTime(_currentTime)
            .ConfigureServices(services => services.AddTransient(_ => _setupLinkCreator));
    }

    private async Task<OnboardUserCommandDto> GenerateValidOnboardingCommand()
    {
        Faker faker = new Faker();
        OnboardUserCommandDtoFaker _onBoardUserCommandFaker = new();
        OrganisationFaker organisationFaker = new();
        var entity = organisationFaker.Generate();
        entity.Id = faker.Random.Int(min: 1_000, max: 1_000_000);
        Organisation organisation = await AddEntity(entity, TestContext.Current.CancellationToken);
        return _onBoardUserCommandFaker.Generate() with { OrganisationId = organisation.Id };
    }

    private sealed class OnboardUserCommandDtoFaker : Faker<OnboardUserCommandDto>
    {
        public OnboardUserCommandDtoFaker()
        {
            UseSeed(12);
            RuleFor(x => x.NewUserEmail, f => f.Internet.Email());
        }
    }
}
