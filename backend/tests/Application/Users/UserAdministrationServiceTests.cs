using Bogus;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
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
    private readonly OnboardUserCommandDtoFaker _onBoardUserCommandFaker = new();
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
        _harness = getTestHarness();
    }

    [Fact]
    public async Task OnboardUser_ShouldCreateANewOnboardingRecordInTheDatabase()
    {
        OnboardUserCommandDto command = _onBoardUserCommandFaker.Generate();
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

        OnboardUserCommandDto command = _onBoardUserCommandFaker.Generate();
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
    public async Task OnBoardUser_ShouldReturnNotAllowedResultUnlessASuperUser()
    {
        IEnumerable<UserRole> noneSuperAdminRoles = Enum.GetValues<UserRole>()
            .Except([UserRole.Super]);

        foreach (var userRole in noneSuperAdminRoles)
        {
            IServiceTestHarness<IUserAdministrationService> harnessWithNoneSuperUserAuth =
                getTestHarness().UpdateCurrentUser(x => x with { UserRole = userRole });
            OnboardUserCommandDto command = _onBoardUserCommandFaker.Generate();
            OnBoardUserResult result = await harnessWithNoneSuperUserAuth.Service.OnboardUser(
                command,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeError().ShouldBeOfType<OnboardUserError.NotAllowed>();
        }
    }

    private IServiceTestHarness<IUserAdministrationService> getTestHarness()
    {
        return new ServiceTestHarness<IUserAdministrationService>(Context)
            .UpdateCurrentUser(x => x with { Email = _currentUserEmail })
            .UpdateCurrentTime(_currentTime)
            .ConfigureServices(services => services.AddTransient(_ => _setupLinkCreator));
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
