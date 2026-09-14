using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.InternalServices.Identity;
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
using RegisterUserConfirmation = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.RegisterUserConfirmationDto,
    UKPS.Api.Application.Users.Errors.RegisterUserError
>;

namespace UKPS.Api.Tests.Application.Users;

[Collection(DatabaseCollection.Name)]
public class UserRegistrationServiceTests : DatabaseTestBase
{
    private readonly IServiceTestHarness<IUserRegistrationService> _harness;
    private readonly RegisterUserCommandDtoFaker _registerUserCommandDtoFaker = new();
    private readonly Faker<Organisation> _organisationFaker;
    private readonly Organisation _organisation;
    private readonly IReadOnlyDictionary<UserRole, User> _mockUserLookup;
    private readonly User _defaultUser;
    private readonly DateTime _currentTime = new DateTime(2023, 12, 12, 2, 4, 12, DateTimeKind.Utc);

    public UserRegistrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _organisationFaker = new OrganisationFaker().RuleFor(
            o => o.Status,
            _ => UserOrgStatus.Active
        );
        _organisation = _organisationFaker
            .RuleFor(o => o.Status, _ => UserOrgStatus.Active)
            .Generate();
        var userMembershipFaker = new UserOrgMembershipFaker().RuleFor(
            x => x.Organisation,
            _ => _organisation
        );
        var userFaker = new UserFaker();
        _mockUserLookup = Enum.GetValues<UserRole>()
            .ToDictionary(
                role => role,
                role =>
                {
                    return userFaker
                        .RuleFor(
                            x => x.UserOrgMemberships,
                            _ =>
                                userMembershipFaker
                                    .RuleFor(x => x.UserRole, _ => role)
                                    .RuleFor(x => x.Organisation, _ => _organisation)
                                    .Generate(1)
                        )
                        .Generate();
                }
            );
        _defaultUser = _mockUserLookup[UserRole.Super];

        _harness = new ServiceTestHarness<IUserRegistrationService>(Context)
            .UpdateCurrentTime(_currentTime)
            .UpdateCurrentUser(ModifyForUser(_defaultUser));
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        await AddEntities(_mockUserLookup.Values, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterUser_AllFieldsProvided_ReturnsDto()
    {
        RegisterUserCommandDto registerUserCommandDto = _registerUserCommandDtoFaker.Generate();
        RegisterUserConfirmation result = await _harness.Service.RegisterUser(
            _organisation.Id,
            registerUserCommandDto,
            TestContext.Current.CancellationToken
        );
        RegisterUserConfirmationDto user = result.ShouldBeSuccess();
        user.ShouldBe(
            new RegisterUserConfirmationDto
            {
                Id = user.Id,
                OrganisationName = user.OrganisationName,
                FullName = registerUserCommandDto.FullName,
                WorkEmail = registerUserCommandDto.WorkEmail,
                PhoneNumber = registerUserCommandDto.PhoneNumber,
            }
        );
    }

    [Fact]
    public async Task RegisterUser_OrganisationNotFound_ReturnsError()
    {
        RegisterUserCommandDto registerUserCommandDto = _registerUserCommandDtoFaker.Generate();
        RegisterUserConfirmation result = await _harness.Service.RegisterUser(
            999,
            registerUserCommandDto,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<RegisterUserError.OrganisationNotFound>();
    }

    [Fact]
    public async Task RegisterUser_SetsCreatedAtOnTheRegistrationEntity()
    {
        RegisterUserCommandDto registerUserCommandDto = _registerUserCommandDtoFaker.Generate();
        RegisterUserConfirmation result = await _harness.Service.RegisterUser(
            _organisation.Id,
            registerUserCommandDto,
            TestContext.Current.CancellationToken
        );
        var data = result.ShouldBeSuccess();
        var entity = await _harness
            .GetClearedContext()
            .UserRegistrationRequests.FindAsync([data.Id], TestContext.Current.CancellationToken);
        entity.ShouldNotBeNull();
        entity.CreatedAt.ShouldBe(_currentTime);
    }

    [Fact]
    public async Task GetUserRegistrationById_UserExists_ReturnsDto()
    {
        UserRegistrationRequest request = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _ => _organisation)
            .Generate();
        Context.UserRegistrationRequests.Add(request);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        GetUserDetails result = await _harness.Service.GetUserRegistrationById(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );

        RegisterUserConfirmationDto user = result.ShouldBeSuccess();

        user.ShouldBe(
            new RegisterUserConfirmationDto
            {
                Id = request.Id,
                OrganisationName = request.Organisation!.OrganisationName,
                FullName = request.FullName,
                WorkEmail = request.WorkEmail,
                PhoneNumber = request.PhoneNumber,
            }
        );
    }

    [Fact]
    public async Task GetUserRegistrationById_UserDoesNotExist_ReturnsIdNotFound()
    {
        int organisationId = 999;
        int id = 999;

        GetUserDetails result = await _harness.Service.GetUserRegistrationById(
            organisationId,
            id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetUserDetailsError.IdNotFound>();
    }

    [Fact]
    public async Task ApproveRequest_ShouldMarkTheRequestAsApproved()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness.Service.ApproveRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        var foundValue = await _harness
            .GetClearedContext()
            .UserRegistrationRequests.Include(x => x.ApprovedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);

        foundValue.ShouldNotBeNull();
        foundValue.GetState().ShouldBe(UserRegistrationRequest.State.Approved);
        foundValue.ApprovedAt.ShouldNotBeNull().ShouldBe(_currentTime);
        foundValue.ApprovedByUser.ShouldNotBeNull().Id.ShouldBe(_defaultUser.Id);
    }

    [Fact]
    public async Task ApproveRequest_WhenAStandardUser_ShouldNotBeAbleToApproveRequests()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness
            .UpdateCurrentUser(ModifyForUser(_mockUserLookup[UserRole.Standard]))
            .Service.ApproveRequest(
                _organisation.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
        result.ShouldBeError().ShouldBeOfType<ApproveRequestError.NotAllowed>();
    }

    [Fact]
    public async Task ApproveRequest_WhenAChampionUser_ShouldOnlyBeAbleToApproveRequestsForMyOrganisation()
    {
        var harness = _harness.UpdateCurrentUser(ModifyForUser(_mockUserLookup[UserRole.Champion]));

        async Task RunTestForMyOrganisation()
        {
            var request = await CreateRegistrationRequest();
            var result = await harness.Service.ApproveRequest(
                _organisation.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeSuccess();
        }

        async Task RunTestForOtherOrganisation()
        {
            var otherOrg = await AddEntity(
                _organisationFaker.Generate(),
                TestContext.Current.CancellationToken
            );
            var request = await CreateRegistrationRequest(organisationOverride: otherOrg.Id);
            var result = await harness.Service.ApproveRequest(
                otherOrg.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeError().ShouldBeOfType<ApproveRequestError.NotAllowed>();
        }

        await RunTestForMyOrganisation();
        await RunTestForOtherOrganisation();
    }

    [Fact]
    public async Task ApproveRequest_ShouldSetupANewUserInTheDatabase()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness.Service.ApproveRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess();

        var user = await _harness
            .GetClearedContext()
            .Users.Include(x => x.UserOrgMemberships)
            .FirstOrDefaultAsync(
                x => x.WorkEmail == request.Command.WorkEmail,
                TestContext.Current.CancellationToken
            );

        user.ShouldNotBeNull();
        var membership = user.UserOrgMemberships.ShouldHaveSingleItem();

        membership.Status.ShouldBe(UserOrgMembershipStatus.AwaitingSetup);
        membership.OrganisationId.ShouldBe(_organisation.Id);
    }

    [Fact]
    public async Task ApproveRequest_ShouldSendTheUserAnEmailNotifyingThemThatTheirRequestHasBeenApproved()
    {
        var request = await CreateRegistrationRequest();
        _ = await _harness.Service.ApproveRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );

        _harness
            .Emails.Sent.ShouldHaveSingleItem()
            .ShouldBeOfType<UserMembershipRequestApprovedNotificationEmail>();
    }

    [Fact]
    public async Task ApproveRequest_WhenOrganisationIdDoesNotExist_ShouldReturnNotFound()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness.Service.ApproveRequest(
            999,
            request.Id,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<ApproveRequestError.RequestNotFound>();
    }

    [Fact]
    public async Task ApproveRequest_RegistrationRequestIdIdDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _harness.Service.ApproveRequest(
            _organisation.Id,
            999,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<ApproveRequestError.RequestNotFound>();
    }

    [Fact]
    public async Task ApproveRequest_WhenRequestHasAlreadyApproved_ShouldShowSuccessAndNotSendANewEmail()
    {
        var request = await CreateRegistrationRequest();
        var resultA = await _harness.Service.ApproveRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );
        var resultB = await _harness.Service.ApproveRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );

        resultA.ShouldBeSuccess();
        resultB.ShouldBeSuccess();

        _harness.Emails.Sent.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RejectRequest_ShouldMarkUserRequestAsRejected()
    {
        var request = await CreateRegistrationRequest();
        await _harness.Service.RejectRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );

        var foundValue = await _harness
            .GetClearedContext()
            .UserRegistrationRequests.Include(x => x.RejectedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);

        foundValue.ShouldNotBeNull();
        foundValue.GetState().ShouldBe(UserRegistrationRequest.State.Rejected);
        foundValue.RejectedAt.ShouldNotBeNull().ShouldBe(_currentTime);
        foundValue.RejectedByUser.ShouldNotBeNull().Id.ShouldBe(_defaultUser.Id);
    }

    [Fact]
    public async Task RejectRequest_ShouldSendTheUserAnEmailNotifyingThem()
    {
        var request = await CreateRegistrationRequest();
        _ = await _harness.Service.RejectRequest(
            _organisation.Id,
            request.Id,
            TestContext.Current.CancellationToken
        );

        _harness
            .Emails.Sent.ShouldHaveSingleItem()
            .ShouldBeOfType<UserMembershipRequestRejectedNotificationEmail>();
    }

    [Fact]
    public async Task RejectRequest_OrganisationIdIdDoesNotExist_ShouldReturnNotFound()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness.Service.RejectRequest(
            999,
            request.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<RejectRequestError.RequestNotFound>();
    }

    [Fact]
    public async Task RejectRequest_RegistrationRequestIdIdDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _harness.Service.RejectRequest(
            _organisation.Id,
            999,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<RejectRequestError.RequestNotFound>();
    }

    [Fact]
    public async Task RejectRequest_WhenAStandardUser_ShouldNotBeAbleToRejectRequests()
    {
        var request = await CreateRegistrationRequest();
        var result = await _harness
            .UpdateCurrentUser(ModifyForUser(_mockUserLookup[UserRole.Standard]))
            .Service.RejectRequest(
                _organisation.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
        result.ShouldBeError().ShouldBeOfType<RejectRequestError.NotAllowed>();
    }

    [Fact]
    public async Task RejectRequest_WhenAChampionUser_ShouldOnlyBeAbleToRejectRequestsForMyOrganisation()
    {
        var harness = _harness.UpdateCurrentUser(ModifyForUser(_mockUserLookup[UserRole.Champion]));

        async Task RunTestForMyOrganisation()
        {
            var request = await CreateRegistrationRequest();
            var result = await harness.Service.RejectRequest(
                _organisation.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeSuccess();
        }

        async Task RunTestForOtherOrganisation()
        {
            var otherOrg = await AddEntity(
                _organisationFaker.Generate(),
                TestContext.Current.CancellationToken
            );
            var request = await CreateRegistrationRequest(organisationOverride: otherOrg.Id);
            var result = await harness.Service.RejectRequest(
                otherOrg.Id,
                request.Id,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeError().ShouldBeOfType<RejectRequestError.NotAllowed>();
        }

        await RunTestForMyOrganisation();
        await RunTestForOtherOrganisation();
    }

    private static Func<CurrentUser, CurrentUser> ModifyForUser(User user)
    {
        return x =>
            x with
            {
                CognitoUsername = user.CognitoUsername,
                UserRole = user.UserOrgMemberships!.First().UserRole,
                Email = user.WorkEmail,
            };
    }

    private async Task<RegistrationContext> CreateRegistrationRequest(
        int? organisationOverride = null,
        Func<RegisterUserCommandDto, RegisterUserCommandDto>? registerUserCommandModifier = null
    )
    {
        RegisterUserCommandDto originalCommand = _registerUserCommandDtoFaker.Generate();
        var modified = registerUserCommandModifier is not null
            ? registerUserCommandModifier(originalCommand)
            : originalCommand;
        RegisterUserConfirmation result = await _harness.Service.RegisterUser(
            organisationOverride ?? _organisation.Id,
            modified,
            TestContext.Current.CancellationToken
        );
        return new(result.ShouldBeSuccess().Id, originalCommand);
    }

    private sealed class RegisterUserCommandDtoFaker : Faker<RegisterUserCommandDto>
    {
        public RegisterUserCommandDtoFaker()
        {
            RuleFor(x => x.FullName, f => f.Name.FullName());
            RuleFor(x => x.WorkEmail, f => f.Internet.Email());
            RuleFor(x => x.PhoneNumber, _ => new TelephoneNumberFaker().Generate());
        }
    }

    private sealed record RegistrationContext(int Id, RegisterUserCommandDto Command);
}
