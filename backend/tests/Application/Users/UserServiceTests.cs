using Bogus;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Common;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;

namespace UKPS.Api.Tests.Application.Users;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests : DatabaseTestBase
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();
    private readonly UpdateUserDetailsCommandFaker _updateUserDetailsCommandFaker = new();
    private IServiceTestHarness<IUserService> _harness;
    private IUserService Service => _harness.Service;

    private readonly DateTime _currentDateTime = new DateTime(
        2003,
        4,
        12,
        12,
        12,
        44,
        DateTimeKind.Utc
    );

    private readonly GetUsersQueryDto _getAllUserQuery = new GetUsersQueryDto() { PageSize = 1000 };
    private readonly Faker _faker = new Faker();
    private readonly IReadOnlyCollection<User> _seededUsers;

    private IEnumerable<User> ViewableUsers =>
        _seededUsers.Where(x => x.UserOrgMemberships!.Any(x => x.Status != UserOrgStatus.Rejected));
    private IEnumerable<UserOrgMembership> SeededMemberships =>
        _seededUsers.SelectMany(x => x.UserOrgMemberships!);
    private IEnumerable<UserOrgMembership> ViewableMemberships =>
        SeededMemberships.Where(x => x.Status != UserOrgStatus.Rejected);

    public UserServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(342);

        _harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentTime(
            _currentDateTime
        );

        var organisations = _organisationFaker.Generate(4);
        var userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            (f, u) =>
            {
                return f.PickRandom(
                        organisations,
                        f.Random.Int(min: 1, max: Math.Min(3, organisations.Count))
                    )
                    .Select(o =>
                        _userOrgMembershipFaker.RuleFor(x => x.Organisation, _ => o).Generate()
                    )
                    .ToArray();
            }
        );
        _seededUsers = userFaker.Generate(50);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await AddEntities(_seededUsers, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnTheDetailsForTheCurrentUser()
    {
        foreach (var _ in Enumerable.Range(0, 10))
        {
            User currentUser = _faker.PickRandom(ViewableUsers);
            UserOrgMembership currentUserMembership = _faker.PickRandom(
                currentUser.UserOrgMemberships!.Where(x => x.Status != UserOrgStatus.Rejected)
            );
            _harness.UpdateCurrentUser(x =>
                x with
                {
                    OrganisationId = currentUserMembership.OrganisationId,
                    UserRole = currentUserMembership.UserRole,
                    Email = currentUser.WorkEmail,
                    CognitoUsername = currentUser.CognitoUsername,
                }
            );
            CurrentUserInformationDto result = await Service.GetCurrentUser(
                TestContext.Current.CancellationToken
            );
            result.ShouldBe(
                new CurrentUserInformationDto()
                {
                    UserId = currentUser.Id,
                    FullName = currentUser.FullName,
                    WorkEmail = currentUser.WorkEmail,
                    WorkTelephone = currentUser.WorkTelephone ?? string.Empty,
                    OrganisationMembershipId = currentUserMembership.Id,
                    OrganisationId = currentUserMembership.OrganisationId,
                    OrganisationName = currentUserMembership.Organisation!.OrganisationName,
                    UserRole = currentUserMembership.UserRole,
                }
            );
        }
    }

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery(organisationId: 99),
            TestContext.Current.CancellationToken
        );

        result.IsErr.ShouldBeTrue();
        GetUsersError.OrganisationNotFound notFound =
            result.Error.ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        var emptyOrg = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery() with
            {
                OrganisationId = emptyOrg.Id,
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        var userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            (f, u) =>
            {
                return _userOrgMembershipFaker
                    .RuleFor(x => x.Organisation, _ => _organisationFaker.Generate())
                    .Generate(1)
                    .ToArray();
            }
        );
        var user = await AddEntity(userFaker.Generate(), TestContext.Current.CancellationToken);
        var userMembership = user.UserOrgMemberships!.Single();
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.Single(x => x.UserId == userMembership.UserId);
        item.UserId.ShouldBe(userMembership.UserId);
        item.EmailAddress.ShouldBe(userMembership.User!.WorkEmail);
        item.Role.ShouldBe(userMembership.UserRole);
        item.Status.ShouldBe(userMembership.Status);

        if (userMembership.User.LastActive.HasValue)
        {
            item.LastActive.HasValue.ShouldBeTrue();
            item.LastActive.Value.ShouldBe(
                userMembership.User.LastActive.Value,
                TimeSpan.FromMicroseconds(1)
            );
        }
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        UserOrgStatus[] filterStatuses = [UserOrgStatus.Active, UserOrgStatus.Inactive];
        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery(status: [UserOrgStatus.Active, UserOrgStatus.Inactive]),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        var statuses = dto.Items.Select(i => i.Status);
        statuses.ShouldOnlyContain(filterStatuses);
        statuses.ShouldContainSet(filterStatuses);
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_ByDefault()
    {
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(ViewableMemberships.Count());
        dto.Items.ShouldAllBe(i => i.Status != UserOrgStatus.Rejected);
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_EvenWhenExplicitlyRequested()
    {
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                Status = [UserOrgStatus.Rejected],
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleRoles_WhenRolesProvided()
    {
        UserRole[] roleFilter = [UserRole.Champion, UserRole.Super];
        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery(role: roleFilter),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        var roles = dto.Items.Select(x => x.Role);
        roles.ShouldOnlyContain(roleFilter);
        roles.ShouldContainSet(roleFilter);
    }

    [Fact]
    public async Task GetUsers_FiltersByPartialEmail_WhenEmailProvided()
    {
        var sampleMembership = _faker.PickRandom(ViewableMemberships);
        var email = sampleMembership.User!.WorkEmail;
        var randomSubString = _faker.GetRandomSubString(email);
        var randomlyCapitalised = _faker.GetRandomlyCapitalisedString(randomSubString);

        GetUsersResult result = await Service.GetUsers(
            new() { Email = randomlyCapitalised, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldContain(x => x.UserId == sampleMembership.User.Id);
        dto.Items.ShouldAllBe(x =>
            x.EmailAddress!.Contains(randomlyCapitalised, StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public async Task GetUsers_TreatsLikeWildcardsLiterally_WhenEmailContainsPercentOrUnderscore()
    {
        Organisation organisation = _organisationFaker.Generate();
        string[] emails = ["100%off@example.com", "jane_doe@example.com", "john.smith@example.com"];
        var data = emails.Select(e =>
        {
            var user = _userFaker.Generate();
            user.Update(x => x.WorkEmail = e);
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery(organisationId: organisation.Id, email: "100%off"),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("100%off@example.com");
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveRange_WhenBothBoundsProvided()
    {
        var sampleUsers = _faker
            .PickRandom(ViewableUsers.Where(x => x.LastActive.HasValue), 3)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (beforeUser, inRangeUser, afterUser) = (sampleUsers[0], sampleUsers[1], sampleUsers[2]);
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                LastActiveFrom = new DateTimeOffset(
                    beforeUser.LastActive!.Value.AddSeconds(1),
                    TimeSpan.Zero
                ),
                LastActiveTo = new DateTimeOffset(
                    afterUser.LastActive!.Value.AddSeconds(-1),
                    TimeSpan.Zero
                ),
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        var ids = dto.Items.Select(x => x.UserId).ToHashSet();
        ids.ShouldNotContain(beforeUser.Id);
        ids.ShouldContain(inRangeUser.Id);
        ids.ShouldNotContain(afterUser.Id);
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveFrom_WhenOnlyFromProvided()
    {
        var sampleUsers = _faker
            .PickRandom(ViewableUsers.Where(x => x.LastActive.HasValue), 2)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (beforeUser, inRangeUser) = (sampleUsers[0], sampleUsers[1]);
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                LastActiveFrom = new DateTimeOffset(
                    beforeUser.LastActive!.Value.AddSeconds(1),
                    TimeSpan.Zero
                ),
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        var ids = dto.Items.Select(x => x.UserId).ToHashSet();
        ids.ShouldNotContain(beforeUser.Id);
        ids.ShouldContain(inRangeUser.Id);
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveTo_WhenOnlyToProvided()
    {
        var sampleUsers = _faker
            .PickRandom(ViewableUsers.Where(x => x.LastActive.HasValue), 2)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (inRangeUser, afterUser) = (sampleUsers[0], sampleUsers[1]);
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                LastActiveTo = new DateTimeOffset(
                    afterUser.LastActive!.Value.AddSeconds(-1),
                    TimeSpan.Zero
                ),
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        var ids = dto.Items.Select(x => x.UserId).ToHashSet();
        ids.ShouldContain(inRangeUser.Id);
        ids.ShouldNotContain(afterUser.Id);
    }

    [Fact]
    public async Task GetUsers_ExcludesNeverActiveUsers_WhenLastActiveFilterProvided()
    {
        _seededUsers.ShouldContain(
            x => x.LastActive.HasValue,
            "Data set should contain at least one user with LastActive value"
        );

        var sampleUser = _faker.PickRandom(ViewableUsers.Where(x => x.LastActive.HasValue));
        GetUsersResult result = await Service.GetUsers(
            CreateGetUsersQuery(
                lastActiveFrom: new DateTimeOffset(sampleUser.LastActive!.Value, TimeSpan.Zero)
            ),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldAllBe(x => x.LastActive.HasValue);
    }

    [Fact]
    public async Task GetUsers_Paginates_WhenUsersExist()
    {
        GetUsersResult result = await Service.GetUsers(
            new() { Page = 2, PageSize = 1 },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(ViewableMemberships.Count());
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
    }

    [Fact]
    public async Task GetUsers_OrdersByUserId()
    {
        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto(),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.Items.Select(x => x.UserId).ShouldBeInOrder();
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                OrganisationId = null,
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.Items.Select(i => _seededUsers.First(x => x.Id == i.UserId))
            .SelectMany(x => x.UserOrgMemberships!.Select(x => x.OrganisationId))
            .Distinct()
            .Count()
            .ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        GetUsersResult withNoFilter = await Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );
        withNoFilter
            .ShouldBeSuccess()
            .Items.Select(x => x.Status)
            .ShouldContainSet([UserOrgStatus.Inactive, UserOrgStatus.Active]);

        GetUsersResult resultWithFilter = await Service.GetUsers(
            _getAllUserQuery with
            {
                Status = [UserOrgStatus.Inactive],
            },
            TestContext.Current.CancellationToken
        );
        resultWithFilter
            .ShouldBeSuccess()
            .Items.Select(x => x.Status)
            .ShouldOnlyContain([UserOrgStatus.Inactive]);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        var pageSize = 5;
        var lastPage = (int)Math.Ceiling((double)ViewableMemberships.Count() / pageSize) + 1;
        GetUsersResult result = await Service.GetUsers(
            new() { Page = lastPage, PageSize = pageSize },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(ViewableMemberships.Count());
        dto.Page.ShouldBe(lastPage);
    }

    [Fact]
    public async Task GetUsers_UserHasMembershipsInMultipleOrganisations_ReturnsOneRowPerMembership()
    {
        var usersWithMultipleMemberships = ViewableUsers.Where(x =>
            x.UserOrgMemberships!.Count > 1
        );
        var sampleUser = _faker.PickRandom(usersWithMultipleMemberships);

        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        var relevantEntries = dto.Items.Where(x => x.UserId == sampleUser.Id);
        relevantEntries.Count().ShouldBe(sampleUser.UserOrgMemberships!.Count);
    }

    [Theory]
    [InlineData(UserRole.Super, false)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Standard, true)]
    public async Task GetUsers_ReturnsAllUsersForSuperAdmins_AndFiltersByOrganisationForOtherRoles(
        UserRole userRole,
        bool filtersByOrganisation
    )
    {
        var organisations = _organisationFaker.Generate(2);
        var users = _userFaker.Generate(3);
        var memberships = new List<UserOrgMembership>
        {
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[0];
                    x.Organisation = organisations[0];
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[1];
                    x.Organisation = organisations[1];
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[2];
                    x.Organisation = organisations[0];
                }),
        };
        await AddEntities(memberships, TestContext.Current.CancellationToken);
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = organisations[0].Id,
                UserRole = userRole,
            }
        );
        var results = await harness.Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );

        var dto = results.ShouldBeSuccess();

        if (filtersByOrganisation)
        {
            dto.TotalCount.ShouldBe(2);
            dto.Items.Select(i => i.UserId).ToArray().ShouldBe([users[0].Id, users[2].Id]);
        }
        else
        {
            dto.Items.Select(i => i.UserId)
                .ShouldContainSet([users[0].Id, users[1].Id, users[2].Id]);
        }
    }

    [Theory]
    [InlineData(UserRole.Super, true)]
    [InlineData(UserRole.Champion, false)]
    [InlineData(UserRole.Standard, false)]
    public async Task GetUsers_ReturnsNotAllowed_WhenExplicitlyRequestingUsersForAnOrganisationIsNotAllowedToAccess(
        UserRole userRole,
        bool isAllowedToAccess
    )
    {
        int userOrganisation = 1;
        int otherOrganisation = 2;
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                UserRole = userRole,
                OrganisationId = userOrganisation,
            }
        );
        IUserService service = harness.Service;
        GetUsersResult result = await service.GetUsers(
            CreateGetUsersQuery(organisationId: otherOrganisation),
            TestContext.Current.CancellationToken
        );

        if (isAllowedToAccess)
        {
            result.Error.ShouldNotBeOfType<GetUsersError.NotAllowed>();
        }
        else
        {
            result.Error.ShouldBeOfType<GetUsersError.NotAllowed>();
        }
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldUpdateUserDetails()
    {
        var currentUser = await CreateExistingCurrentUser();
        var command = _updateUserDetailsCommandFaker.Generate();
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        var databaseUser = await Context.Users.FindAsync(
            [currentUser.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        var databaseValues = new UpdateUserDetailsCommand()
        {
            FullName = databaseUser.FullName,
            WorkEmail = databaseUser.WorkEmail,
            WorkTelephone = databaseUser.WorkTelephone ?? string.Empty,
        };
        databaseValues.ShouldBe(command);
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldUpdateUpdatedAtTime()
    {
        var currentUser = await CreateExistingCurrentUser();
        var command = _updateUserDetailsCommandFaker.Generate();
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        var databaseUser = await Context.Users.FindAsync(
            [currentUser.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        databaseUser.UpdatedAt.ShouldBe(_currentDateTime);
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldReturnUpdatedUserDetails()
    {
        User currentUser = await CreateExistingCurrentUser();
        UpdateUserDetailsCommand command = _updateUserDetailsCommandFaker.Generate();
        Result<UserDetailsDto, UpdateUserDetailsError> result = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        UserDetailsDto value = result.ShouldBeSuccess();
        var responseValues = new UpdateUserDetailsCommand()
        {
            FullName = value.FullName,
            WorkEmail = value.WorkEmail,
            WorkTelephone = value.WorkPhone ?? string.Empty,
        };
        responseValues.ShouldBe(command);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenEmailConflictsWithExistingUser_ShouldReturnAnError()
    {
        var existingOtherUser = await AddEntity(
            _userFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        User currentUser = await CreateExistingCurrentUser();
        UpdateUserDetailsCommand command = _updateUserDetailsCommandFaker.Generate() with
        {
            WorkEmail = existingOtherUser.WorkEmail,
        };
        Result<UserDetailsDto, UpdateUserDetailsError> result = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UpdateUserDetailsError.ConflictingEmail>();
    }

    [Fact]
    public async Task UpdateUserDetails_WhenEmailChanges_ShouldUpdateEmailAttributeInCognito()
    {
        User currentUser = await CreateExistingCurrentUser();
        string testEmail = "testupdateuserdetails@email.com";
        UpdateUserDetailsCommand command = _updateUserDetailsCommandFaker.Generate() with
        {
            WorkEmail = testEmail,
        };
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );
        MockUser? user = _harness.Cognito.GetUserByEmail(testEmail);
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateUserDetails_WhenUserDoesNotExist_ShouldReturnUserDoesNotExistError()
    {
        var result = await Service.UpdateUserDetails(
            999,
            _updateUserDetailsCommandFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UpdateUserDetailsError.UserDoesNotExist>();
    }

    [Fact]
    public async Task UpdateUserDetails_WhenUserIsNotTheTargetUser_ShouldReturnUserIsNoPermittedToUserDetailsError()
    {
        var currentUser = await CreateExistingCurrentUser();
        var otherUserTestHarness = new ServiceTestHarness<IUserService>(_harness).UpdateCurrentUser(
            x => x with { Email = "otheruser@email.com" }
        );
        var command = _updateUserDetailsCommandFaker.Generate();
        var result = await otherUserTestHarness.Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UpdateUserDetailsError.Unauthorised>();
    }

    private async Task<User> CreateExistingCurrentUser()
    {
        Organisation org = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        IUserAdministrationService userAdministrationService =
            new ServiceTestHarness<IUserAdministrationService>(_harness).Service;
        Faker<OnboardUserCommandDto> onboardingUserFaker = new OnboardUserCommandDtoFaker().RuleFor(
            x => x.OrganisationId,
            _ => org.Id
        );
        Result<int, OnboardUserError> result = await userAdministrationService.OnboardUser(
            onboardingUserFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        return result.Match(
            x =>
            {
                var user =
                    Context.Users.Find(x)
                    ?? throw new InvalidOperationException("Could not find created user.");
                _harness = _harness.UpdateCurrentUser(x => x with { Email = user.WorkEmail });

                return user;
            },
            (e) => throw new InvalidOperationException("Failed to create an initial user")
        );
    }

    private static GetUsersQueryDto CreateGetUsersQuery(
        int? organisationId = 1,
        int page = 1,
        int pageSize = 20,
        ICollection<UserOrgStatus>? status = null,
        ICollection<UserRole>? role = null,
        string? email = null,
        DateTimeOffset? lastActiveFrom = null,
        DateTimeOffset? lastActiveTo = null
    ) =>
        new()
        {
            OrganisationId = organisationId,
            Page = page,
            PageSize = pageSize,
            Status = status ?? [],
            Role = role ?? [],
            Email = email,
            LastActiveFrom = lastActiveFrom,
            LastActiveTo = lastActiveTo,
        };
}
