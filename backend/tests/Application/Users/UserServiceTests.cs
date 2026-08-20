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
using UsersQueryResult = UKPS.Api.Application.Common.Result<
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
    private readonly IUserService _service;
    private readonly Randomizer _rng = new Randomizer(6);

    private IReadOnlyCollection<User> _seededUsers = null!;
    private IReadOnlyCollection<User> NonRejectedSeededUsers =>
        _seededUsers
            .Where(x => x.UserOrgMemberships!.Any(m => m.Status != UserOrgStatus.Rejected))
            .ToArray();
    private UserOrgMembership[] NonRejectedMemberships =>
        _seededUsers
            .SelectMany(x => x.UserOrgMemberships!)
            .Where(x => x.Status != UserOrgStatus.Rejected)
            .ToArray();

    public UserServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _service = new ServiceTestHarness<IUserService>(Context).Service;
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var organisations = _organisationFaker.Generate(3);
        var userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            f =>
            {
                return f.PickRandom(organisations, f.Random.Int(1, 3))
                    .Select(o =>
                        _userOrgMembershipFaker.RuleFor(x => x.Organisation, _ => o).Generate()
                    )
                    .ToArray();
            }
        );
        var users = userFaker.Generate(50);
        _seededUsers = (await AddEntities(users, TestContext.Current.CancellationToken)).ToList();
    }

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        UsersQueryResult result = await _service.GetUsers(
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
        var organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(organisationId: organisation.Id),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
        dto.Page.ShouldBe(1);
        dto.PageSize.ShouldBe(20);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        var user = _rng.ArrayElement(
            _seededUsers.Where(x => x.UserOrgMemberships!.Count == 1).ToArray()
        );
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(pageSize: 1000),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        var membership = user.UserOrgMemberships!.Single();
        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.Single(x => x.UserId == user.Id);
        item.UserId.ShouldBe(user.Id);
        item.EmailAddress.ShouldBe(user.WorkEmail);
        item.Role.ShouldBe(membership.UserRole);
        item.Status.ShouldBe(membership.Status);
        if (user.LastActive.HasValue)
        {
            item.LastActive.ShouldNotBeNull();
            item.LastActive.Value.ShouldBe(user.LastActive.Value, TimeSpan.FromSeconds(1));
        }
        else
        {
            item.LastActive.ShouldBeNull();
        }
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        UserOrgStatus[] filteredUserOrganisationIds =
        [
            UserOrgStatus.Active,
            UserOrgStatus.Inactive,
        ];
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(status: [UserOrgStatus.Active, UserOrgStatus.Inactive]),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldAllBe(x => filteredUserOrganisationIds.Contains(x.Status));
        foreach (var status in filteredUserOrganisationIds)
        {
            dto.Items.ShouldContain(x => x.Status == status);
        }
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_ByDefault()
    {
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();

        dto.Items.ShouldAllBe(i => i.Status != UserOrgStatus.Rejected);
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_EvenWhenExplicitlyRequested()
    {
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(status: [UserOrgStatus.Rejected]),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleRoles_WhenRolesProvided()
    {
        UserRole[] filteredValues = [UserRole.Champion, UserRole.Super];
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(role: filteredValues),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        var itemRoles = dto.Items.Select(x => x.Role);
        itemRoles.ShouldAllBe(x => filteredValues.Contains(x));
        foreach (var role in filteredValues)
        {
            itemRoles.ShouldContain(role);
        }
    }

    [Fact]
    public async Task GetUsers_FiltersByPartialEmail_WhenEmailProvided()
    {
        var sampleUser = _rng.ArrayElement(NonRejectedSeededUsers.ToArray());

        var queryString = _rng.RandomSubstring(sampleUser.WorkEmail);
        queryString = _rng.RandomizeCharacterCasing(queryString);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(email: queryString, pageSize: 1000),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldContain(x => x.UserId == sampleUser.Id);
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
            var membership = _userOrgMembershipFaker
                .RuleFor(x => x.Status, _ => UserOrgStatus.Active)
                .Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(email: "100%off"),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("100%off@example.com");
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveRange_WhenBothBoundsProvided()
    {
        var activeSampleUsers = NonRejectedSeededUsers
            .Where(x => x.LastActive is not null)
            .ToArray();
        var sampleUsers = _rng.ArrayElements(activeSampleUsers, 3)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (beforeUser, inRangeUser, afterUser) = (sampleUsers[0], sampleUsers[1], sampleUsers[2]);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(
                lastActiveFrom: new DateTimeOffset(
                    beforeUser.LastActive!.Value.AddSeconds(1),
                    TimeSpan.Zero
                ),
                lastActiveTo: new DateTimeOffset(
                    afterUser.LastActive!.Value.AddSeconds(-1),
                    TimeSpan.Zero
                ),
                pageSize: 1000
            ),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldNotContain(x => x.UserId == afterUser.Id);
        dto.Items.ShouldContain(x => x.UserId == inRangeUser.Id);
        dto.Items.ShouldNotContain(x => x.UserId == beforeUser.Id);
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveFrom_WhenOnlyFromProvided()
    {
        var activeSampleUsers = NonRejectedSeededUsers
            .Where(x => x.LastActive is not null)
            .ToArray();
        var sampleUsers = _rng.ArrayElements(activeSampleUsers, 2)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (beforeUser, afterUser) = (sampleUsers[0], sampleUsers[1]);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(
                lastActiveFrom: new DateTimeOffset(
                    beforeUser.LastActive!.Value.AddSeconds(1),
                    TimeSpan.Zero
                ),
                pageSize: 1000
            ),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldContain(x => x.UserId == afterUser.Id);
        dto.Items.ShouldNotContain(x => x.UserId == beforeUser.Id);
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveTo_WhenOnlyToProvided()
    {
        var activeSampleUsers = NonRejectedSeededUsers
            .Where(x => x.LastActive is not null)
            .ToArray();
        var sampleUsers = _rng.ArrayElements(activeSampleUsers, 2)
            .OrderBy(x => x.LastActive)
            .ToArray();
        var (beforeUser, afterUser) = (sampleUsers[0], sampleUsers[1]);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(
                lastActiveTo: new DateTimeOffset(
                    afterUser.LastActive!.Value.AddSeconds(-1),
                    TimeSpan.Zero
                ),
                pageSize: 1000
            ),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldNotContain(x => x.UserId == afterUser.Id);
        dto.Items.ShouldContain(x => x.UserId == beforeUser.Id);
    }

    [Fact]
    public async Task GetUsers_ExcludesNeverActiveUsers_WhenLastActiveFilterProvided()
    {
        var activeSampleUsers = NonRejectedSeededUsers
            .Where(x => x.LastActive is not null)
            .ToArray();
        var sampleUser = _rng.ArrayElement(activeSampleUsers);

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(
                lastActiveFrom: new DateTimeOffset(sampleUser.LastActive!.Value, TimeSpan.Zero)
            ),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldAllBe(x => x.LastActive.HasValue);
    }

    [Fact]
    public async Task GetUsers_PaginatesAndOrdersByUserId_WhenUsersExist()
    {
        var pageNumber = 2;
        var pageSize = 1;
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(page: pageNumber, pageSize: pageSize),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(NonRejectedMemberships.Length);
        dto.Page.ShouldBe(pageNumber);
        dto.PageSize.ShouldBe(pageSize);
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(organisationId: null),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.Items.SelectMany(x =>
                LookUpDatabaseEntity(x.UserId).UserOrgMemberships!.Select(m => m.OrganisationId)
            )
            .Distinct()
            .Count()
            .ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(organisationId: null, status: [UserOrgStatus.Inactive]),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldAllBe(x => x.Status == UserOrgStatus.Inactive);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        var pageSize = 20;
        var firstResult = await _service.GetUsers(
            CreateGetUsersQuery(pageSize: pageSize),
            TestContext.Current.CancellationToken
        );
        var firstDto = firstResult.ShouldBeSuccess();
        var finalPage = (int)Math.Ceiling((double)firstDto.TotalCount / firstDto.PageSize) + 1;

        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(pageSize: pageSize, page: finalPage),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(firstDto.TotalCount);
        dto.Page.ShouldBe(finalPage);
    }

    [Fact]
    public async Task GetUsers_UserHasMembershipsInMultipleOrganisations_ReturnsOneRowPerMembership()
    {
        var usersWithMultipleMemberships = NonRejectedSeededUsers
            .Where(x => x.UserOrgMemberships!.Count(m => m.Status != UserOrgStatus.Rejected) > 1)
            .ToArray();
        var sampleUser = _rng.ArrayElement(usersWithMultipleMemberships);
        UsersQueryResult result = await _service.GetUsers(
            CreateGetUsersQuery(organisationId: null, pageSize: 1000),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        var userEntries = dto.Items.Where(x => x.UserId == sampleUser.Id);
        var expectedNumberOfEntries = sampleUser.UserOrgMemberships!.Count(x =>
            x.Status != UserOrgStatus.Rejected
        );
        userEntries.Count().ShouldBe(expectedNumberOfEntries);
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
        var sampleUser = _rng.ArrayElement(NonRejectedSeededUsers.ToArray());
        var sampleOrgId = sampleUser.UserOrgMemberships!.First().Organisation!.Id;
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = sampleUser.UserOrgMemberships!.First().Organisation!.Id,
                UserRole = userRole,
            }
        );
        var results = await harness.Service.GetUsers(
            CreateGetUsersQuery(organisationId: null, pageSize: 1000),
            TestContext.Current.CancellationToken
        );

        var dto = results.ShouldBeSuccess();

        if (filtersByOrganisation)
        {
            dto.Items.Select(x => LookUpDatabaseEntity(x.UserId))
                .ShouldNotContain(x =>
                    !x.UserOrgMemberships!.Any(m => m.OrganisationId == sampleOrgId)
                );
        }
        else
        {
            dto.Items.Select(x => LookUpDatabaseEntity(x.UserId))
                .ShouldContain(x =>
                    !x.UserOrgMemberships!.Any(m => m.OrganisationId == sampleOrgId)
                );
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
        UsersQueryResult result = await service.GetUsers(
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

    private User LookUpDatabaseEntity(int userId)
    {
        return _seededUsers.First(x => x.Id == userId);
    }

    private static GetUsersQueryDto CreateGetUsersQuery(
        int? organisationId = null,
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
