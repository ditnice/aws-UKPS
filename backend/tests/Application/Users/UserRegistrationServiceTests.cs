using Bogus;
using Shouldly;
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
    private Organisation _organisation = null!;
    private readonly DateTime _currentTime = new DateTime(2023, 12, 12, 2, 4, 12, DateTimeKind.Utc);

    public UserRegistrationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<IUserRegistrationService>(Context).UpdateCurrentTime(
            _currentTime
        );
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        OrganisationFaker organisationFaker = new();
        _organisation = organisationFaker
            .RuleFor(o => o.Status, _ => UserOrgStatus.Active)
            .Generate();

        var context = _harness.GetClearedContext();
        context.Organisations.Add(_organisation);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
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
        UserRegistrationRequest request = new UserRegistrationRequestFaker().Generate();
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

    private sealed class RegisterUserCommandDtoFaker : Faker<RegisterUserCommandDto>
    {
        public RegisterUserCommandDtoFaker()
        {
            RuleFor(x => x.FullName, f => f.Name.FullName());
            RuleFor(x => x.WorkEmail, f => f.Internet.Email());
            RuleFor(x => x.PhoneNumber, _ => new TelephoneNumberFaker().Generate());
        }
    }
}
