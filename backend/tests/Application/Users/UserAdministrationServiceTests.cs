using Amazon.CognitoIdentityProvider.Model;
using Bogus;
using Microsoft.EntityFrameworkCore;
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
using GetUserDetails = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.RegisterUserConfirmationDto,
    UKPS.Api.Application.Users.Errors.GetUserDetailsError
>;
using OnboardUserResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.Users.Errors.OnboardUserError>;
using RegisterUserConfirmation = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.RegisterUserConfirmationDto,
    UKPS.Api.Application.Users.Errors.RegisterUserError
>;

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
    private readonly string _targetUserEmail = "target.user@email.com";
    private readonly string _currentUserEmail = "current.user@email.com";
    private readonly ISetupLinkCreator _setupLinkCreator = Substitute.For<ISetupLinkCreator>();
    private readonly Faker<MockUser> _mockUserFaker =
        new MockAmazonCognitoIdentityProvider.MockUserFaker();
    private readonly RegisterUserDtoFaker _registerUserDtoFaker = new();

    public UserAdministrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = GetTestHarness();
    }

    [Fact]
    public async Task OnboardUser_ShouldCreateANewOnboardingRecordInTheDatabase()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnboardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        var foundUserRecord = await _harness
            .GetClearedContext()
            .UserOnboardingRecords.SingleOrDefaultAsync(
                x => x.User!.WorkEmail == command.NewUserEmail,
                TestContext.Current.CancellationToken
            );

        foundUserRecord.ShouldNotBeNull();
        foundUserRecord.CreatedAt.ShouldBe(_currentTime);
        foundUserRecord.CreatedBy.ShouldBe(_currentUserEmail);

        _harness.Cognito.Users.ShouldContain(x => x.Username == command.NewUserEmail);
    }

    [Fact]
    public async Task OnboardUser_ShouldCreateANewCognitoUser()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnboardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        _harness.Cognito.Users.ShouldContain(x => x.Username == command.NewUserEmail);
    }

    [Fact]
    public async Task OnboardUser_ShouldCreateANewUserInTheDatabase()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnboardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        User? foundUser = await _harness
            .GetClearedContext()
            .Users.Include(x => x.UserOrgMemberships)
            .SingleOrDefaultAsync(
                x => x.WorkEmail == command.NewUserEmail,
                TestContext.Current.CancellationToken
            );

        foundUser.ShouldNotBeNull();
        foundUser.IdentityId.ShouldNotBeNull();
        foundUser.CreatedAt.ShouldBe(_currentTime);
        foundUser.FullName.ShouldBe(command.FullName);
        foundUser.WorkTelephone.ShouldBe(command.ContactNumber);
        var membership = foundUser.UserOrgMemberships.ShouldHaveSingleItem();
        membership.CreatedAt.ShouldBe(_currentTime);
        membership.OrganisationId.ShouldBe(command.OrganisationId);
        membership.UserRole.ShouldBe(UserRole.Standard);
    }

    [Fact]
    public async Task OnboardUser_ShouldSendAUserSignUpRequestEmailIncludingALinkGeneratedFromTheSetupLinkCreator()
    {
        var testLink = new Uri("https://example.com");
        _setupLinkCreator.GetSetupLink(Arg.Any<Guid>()).Returns(testLink);

        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnboardUserResult result = await _harness.Service.OnboardUser(
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
    public async Task OnboardUser_ForOtherOtherOrganisations_ShouldReturnNotAllowedResultUnlessASuperUser()
    {
        IEnumerable<UserRole> noneSuperAdminRoles = Enum.GetValues<UserRole>()
            .Except([UserRole.Super]);

        foreach (var userRole in noneSuperAdminRoles)
        {
            IServiceTestHarness<IUserAdministrationService> harnessWithNoneSuperUserAuth =
                GetTestHarness().UpdateCurrentUser(x => x with { UserRole = userRole });
            OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
            OnboardUserResult result = await harnessWithNoneSuperUserAuth.Service.OnboardUser(
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
    public async Task OnboardUser_ForSameOrganisation_ShouldReturnNotAllowedResultUnlessASuperUserOrChampion(
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
        OnboardUserResult result = await harnessWithNoneSuperUserAuth.Service.OnboardUser(
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
    public async Task OnboardUser_ShouldReturnInvalidOrganisationErrorIfReferencingOrganisationThatDoesNotExist()
    {
        OnboardUserCommandDto command = await GenerateValidOnboardingCommand() with
        {
            OrganisationId = 999,
        };
        OnboardUserResult result = await _harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<OnboardUserError.InvalidOrganisation>();
    }

    [Fact]
    public async Task OnboardUser_WhenUsernameAlreadyInUse_ShouldReturnUsernameAlreadyInUseResult()
    {
        var harness = GetTestHarness();
        harness
            .Cognito.Mock.WhenForAnyArgs(x => x.AdminCreateUserAsync(default!, default!))
            .Throws(new UsernameExistsException());

        OnboardUserCommandDto command = await GenerateValidOnboardingCommand();
        OnboardUserResult result = await harness.Service.OnboardUser(
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<OnboardUserError.UsernameAlreadyExists>();
    }

    [Fact]
    public async Task RegisterUser_AllFieldsProvided_ReturnsDto()
    {
        RegisterUserDto registerUserDto = _registerUserDtoFaker.Generate();
        RegisterUserConfirmation result = await _harness.Service.RegisterUser(
            registerUserDto,
            TestContext.Current.CancellationToken
        );
        RegisterUserConfirmationDto user = result.ShouldBeSuccess();
        user.ShouldBe(
            new RegisterUserConfirmationDto
            {
                Id = user.Id,
                OrganisationName = user.OrganisationName,
                FullName = registerUserDto.FullName,
                WorkEmail = registerUserDto.WorkEmail,
                PhoneNumber = registerUserDto.PhoneNumber,
            }
        );
    }

    [Fact]
    public async Task GetUserDetailsById_UserExists_ReturnsDto()
    {
        OrganisationFaker organisationFaker = new();
        var organisation = organisationFaker.Generate();
        Context.Organisations.Add(organisation);
        UserRegistrationRequest request = new()
        {
            Organisation = organisation,
            OrganisationId = organisation.Id,
            FullName = "Test",
            WorkEmail = "test@example.com",
            PhoneNumber = "07654281622",
        };
        Context.UserRegistrationRequest.Add(request);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        GetUserDetails result = await _harness.Service.GetUserDetailsById(
            request.Id,
            TestContext.Current.CancellationToken
        );

        RegisterUserConfirmationDto user = result.ShouldBeSuccess();

        user.ShouldBe(
            new RegisterUserConfirmationDto
            {
                Id = request.Id,
                OrganisationName = organisation.OrganisationName,
                FullName = request.FullName,
                WorkEmail = request.WorkEmail,
                PhoneNumber = request.PhoneNumber,
            }
        );
    }

    [Fact]
    public async Task GetUserDetailsById_UserDoesNotExist_ReturnsIdNotFound()
    {
        int id = 999;

        GetUserDetails result = await _harness.Service.GetUserDetailsById(
            id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetUserDetailsError.IdNotFound>();
    }

    private IServiceTestHarness<IUserAdministrationService> GetTestHarness()
    {
        var harness = new ServiceTestHarness<IUserAdministrationService>(Context)
            .UpdateCurrentUser(x => x with { Email = _currentUserEmail })
            .UpdateCurrentTime(_currentTime)
            .ConfigureServices(services => services.AddTransient(_ => _setupLinkCreator));

        harness.Cognito.AddCurrentUser(
            _mockUserFaker.Generate() with
            {
                Username = _targetUserEmail,
            }
        );

        return harness;
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
            RuleFor(x => x.FullName, f => f.Name.FullName());
            RuleFor(x => x.ContactNumber, f => f.Phone.PhoneNumber());
            RuleFor(x => x.NewUserEmail, f => f.Internet.Email());
        }
    }

    private sealed class RegisterUserDtoFaker : Faker<RegisterUserDto>
    {
        public RegisterUserDtoFaker()
        {
            RuleFor(x => x.FullName, f => f.Name.FullName());
            RuleFor(x => x.WorkEmail, f => f.Internet.Email());
            RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber());
        }
    }
}
