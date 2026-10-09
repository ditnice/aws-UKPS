using System.Data.Common;
using Amazon.CognitoIdentityProvider.Model;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Common;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetUserInformationResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Users.Dtos.UserInformationDto,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;
using SortDirection = UKPS.Api.Application.Common.SortDirection;

namespace UKPS.Api.Tests.Application.Users;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests : DatabaseTestBase
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly Faker<MockUser> _mockUserFaker =
        new MockAmazonCognitoIdentityProvider.MockUserFaker();
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
    private readonly IReadOnlyCollection<UserRegistrationRequest> _userRegistrationRequests;
    private readonly IReadOnlyCollection<User> _seededUsers;

    private IEnumerable<User> ViewableUsers => _seededUsers;
    private IEnumerable<UserOrgMembership> SeededMemberships =>
        _seededUsers.SelectMany(x => x.UserOrgMemberships!);
    private IEnumerable<UserOrgMembership> ViewableMemberships => SeededMemberships;
    private int TotalExpectedValues =>
        ViewableMemberships.Count() + _userRegistrationRequests.Count(x => x.RejectedAt is null);

    public UserServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(342);

        _harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentTime(
            _currentDateTime
        );

        var organisations = _organisationFaker.Generate(3);
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
        _userRegistrationRequests = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, f => f.PickRandom(organisations))
            .Generate(20);
        _seededUsers = userFaker.Generate(50);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await AddEntities(_seededUsers, TestContext.Current.CancellationToken);
        await AddEntities(_userRegistrationRequests, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnTheDetailsForTheCurrentUser()
    {
        foreach (var _ in Enumerable.Range(0, 10))
        {
            User currentUser = _faker.PickRandom(ViewableUsers);
            UserOrgMembership currentUserMembership = _faker.PickRandom(
                currentUser.UserOrgMemberships
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
            UserInformationDto result = await Service.GetCurrentUser(
                TestContext.Current.CancellationToken
            );
            result.ShouldBe(
                new UserInformationDto()
                {
                    UserId = currentUser.Id,
                    FullName = currentUser.FullName,
                    WorkEmail = currentUser.WorkEmail,
                    WorkTelephone = currentUser.WorkTelephone ?? string.Empty,
                    OrganisationMembershipId = currentUserMembership.Id,
                    OrganisationId = currentUserMembership.OrganisationId,
                    OrganisationName = currentUserMembership.Organisation!.OrganisationName,
                    UserRole = currentUserMembership.UserRole,
                    Status = currentUserMembership.Status.ConvertToUserOrgStatus(),
                }
            );
        }
    }

    [Fact]
    public async Task GetUsers_WhenThereAreMultipleMembershipRequests_ShouldGetTheLatestOne()
    {
        var userEmail = "example563@email.com";
        var organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        var faker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _ => organisation)
            .RuleFor(x => x.WorkEmail, _ => userEmail)
            .RuleFor(x => x.RejectedAt, _ => null)
            .RuleFor(x => x.RejectedBy, _ => null);
        var registrationRequests = await AddEntities(
            faker.Generate(5),
            TestContext.Current.CancellationToken
        );
        var latestRequest = registrationRequests.OrderBy(x => x.CreatedAt).Last();
        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto() { OrganisationId = organisation.Id },
            TestContext.Current.CancellationToken
        );
        var data = result.ShouldBeSuccess();
        data.Items.ShouldHaveSingleItem()
            .RegistrationRequestGuid.ShouldBe(latestRequest.RequestGuid);
    }

    [Fact]
    public async Task GetUsers_WhenThereIsARejectedMembershipRequest_ShouldNotShowThatRequestAsAUser()
    {
        var organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        var faker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _ => organisation)
            .FinishWith(
                (f, x) =>
                    x.Reject(
                        _userFaker.Generate(),
                        DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc)
                    )
            );
        await AddEntity(faker.Generate(), TestContext.Current.CancellationToken);

        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto() { OrganisationId = organisation.Id },
            TestContext.Current.CancellationToken
        );
        var data = result.ShouldBeSuccess();
        data.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUsers_WhenThereIsAnApprovedMembershipRequest_ShouldNotShowThatRequestAsAUser()
    {
        var organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        var faker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _ => organisation)
            .FinishWith(
                (f, x) =>
                    x.Approve(
                        _userFaker.Generate(),
                        DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc)
                    )
            );
        await AddEntity(faker.Generate(), TestContext.Current.CancellationToken);

        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto() { OrganisationId = organisation.Id },
            TestContext.Current.CancellationToken
        );
        var data = result.ShouldBeSuccess();
        data.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUsers_ReturnsUserWaitingForAccessToBeGranted()
    {
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );

        var data = result.ShouldBeSuccess();
        data.Items.ShouldContain(x => x.Status == UserOrgStatus.RequestedAccess);
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
    public async Task GetUsers_WhenSortParametersNotSet_ShouldDefaultToSortingByLastActiveDescending()
    {
        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto(),
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        var lastActiveValues = dto.Items.Select(x => x.LastActive).ToArray();
        lastActiveValues.ShouldBeInOrder(Shouldly.SortDirection.Descending);
    }

    [Fact]
    public async Task GetUsers_WhenSortingByLastActive_NoValueSetIsTreatedAsALowValue()
    {
        GetUsersResult result = await Service.GetUsers(
            _getAllUserQuery with
            {
                SortBy = GetUsersQuerySortValue.LastActive,
                SortDirection = SortDirection.Ascending,
                PageSize = 1000,
            },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        int greatestIndexOfTheUsersWhereLastActiveIsNull = dto
            .Items.Enumerate()
            .Where(x => !x.Value.LastActive.HasValue)
            .Max(x => x.Index);
        int minIndexOfTheUsersWhereLastActiveIsSet = dto
            .Items.Enumerate()
            .Where(x => x.Value.LastActive.HasValue)
            .Min(x => x.Index);
        greatestIndexOfTheUsersWhereLastActiveIsNull.ShouldBeLessThan(
            minIndexOfTheUsersWhereLastActiveIsSet
        );
    }

    [Fact]
    public async Task GetUsers_WhenThereAreTwoMembershipRequestsForTwoDifferenceOrganisations_ShouldShowBothMembershipRequests()
    {
        var email = "example@email.com";
        var userMembershipRequests = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, _ => _organisationFaker.Generate())
            .RuleFor(x => x.WorkEmail, _ => email)
            .Generate(2);
        await AddEntities(userMembershipRequests, TestContext.Current.CancellationToken);

        foreach (var item in userMembershipRequests)
        {
            GetUsersResult result = await Service.GetUsers(
                new GetUsersQueryDto() { OrganisationId = item.OrganisationId },
                TestContext.Current.CancellationToken
            );
            PaginatedResponseDto<UserListItemDto> data = result.ShouldBeSuccess();
            data.Items.ShouldHaveSingleItem().RegistrationRequestGuid.ShouldBe(item.RequestGuid);
        }
    }

    [Fact]
    public async Task GetUsers_WhenLatestMembershipRequestIsRejected_ShouldReturnNoUsersRelatedToThatMembershipRequest()
    {
        var email = "example@email.com";
        var referenceDateTime = new DateTime(2023, 4, 4, 12, 45, 0, DateTimeKind.Utc);
        var organisation = _organisationFaker.Generate();
        var userMembershipRequestsFaker = new UserRegistrationRequestFaker()
            .RuleFor(x => x.WorkEmail, _ => email)
            .RuleFor(x => x.Organisation, _ => organisation);
        var membershipRequestA = userMembershipRequestsFaker
            .RuleFor(x => x.CreatedAt, referenceDateTime.AddHours(-2))
            .Generate();
        var membershipRequestB = userMembershipRequestsFaker
            .RuleFor(x => x.CreatedAt, referenceDateTime.AddHours(-1))
            .RuleFor(x => x.RejectedAt, _ => referenceDateTime.AddHours(-0.5))
            .RuleFor(x => x.RejectedByUser, _ => _userFaker.Generate())
            .Generate();
        await AddEntities(
            [membershipRequestA, membershipRequestB],
            TestContext.Current.CancellationToken
        );
        GetUsersResult result = await Service.GetUsers(
            new GetUsersQueryDto() { OrganisationId = organisation.Id },
            TestContext.Current.CancellationToken
        );
        result.ShouldBeSuccess().Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUsers_WhenSortParametersSet_ShouldSortBySpecifiedField()
    {
        var getterLookup = new Dictionary<GetUsersQuerySortValue, Func<UserListItemDto, object?>>()
        {
            { GetUsersQuerySortValue.LastActive, x => x.LastActive },
            { GetUsersQuerySortValue.Email, x => x.EmailAddress },
            { GetUsersQuerySortValue.Role, x => x.Role },
            { GetUsersQuerySortValue.Status, x => x.Status },
        };

        foreach (var sortableValue in Enum.GetValues<GetUsersQuerySortValue>())
        {
            var getter = getterLookup.TryGetValue(sortableValue, out var g)
                ? g
                : throw new InvalidOperationException($"No getter defined for {sortableValue}");

            var baseQuery = new GetUsersQueryDto() { SortBy = sortableValue };

            GetUsersResult resultAsc = await Service.GetUsers(
                baseQuery with
                {
                    SortDirection = SortDirection.Ascending,
                },
                TestContext.Current.CancellationToken
            );
            var dataAsc = resultAsc.ShouldBeSuccess();
            dataAsc.Items.Select(getter).ShouldBeInOrder(Shouldly.SortDirection.Ascending);

            GetUsersResult resultDesc = await Service.GetUsers(
                baseQuery with
                {
                    SortDirection = SortDirection.Descending,
                },
                TestContext.Current.CancellationToken
            );
            var dataDesc = resultDesc.ShouldBeSuccess();
            dataDesc.Items.Select(getter).ShouldBeInOrder(Shouldly.SortDirection.Descending);
        }
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
        item.Status.ShouldBe(userMembership.Status.ConvertToUserOrgStatus());

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
        var randomSubString = _faker.GetRandomSubString(email, minLength: 2);
        var randomlyCapitalised = _faker.GetRandomlyCapitalisedString(randomSubString);

        GetUsersResult result = await Service.GetUsers(
            new() { Email = randomlyCapitalised, PageSize = 1000 },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldContain(x => x.UserId == sampleMembership.User.Id);
        dto.Items.ShouldAllBe(x =>
            x.EmailAddress.Contains(randomlyCapitalised, StringComparison.OrdinalIgnoreCase)
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
        dto.TotalCount.ShouldBe(TotalExpectedValues);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
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
        dto.Items.SelectMany(i => GetOrganisationIdsFromUserEmails(i.EmailAddress))
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
        var lastPage = (int)Math.Ceiling((double)TotalExpectedValues / pageSize) + 1;
        GetUsersResult result = await Service.GetUsers(
            new() { Page = lastPage, PageSize = pageSize },
            TestContext.Current.CancellationToken
        );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(TotalExpectedValues);
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
    public async Task GetUsers_WhenChampionOrSuperUser_ReturnsUsersForTheirPermittedOrganisations(
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
            // users[2] belongs to both organisations, so a row for organisations[1] must not
            // be returned to a champion of organisations[0].
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[2];
                    x.Organisation = organisations[1];
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
            dto.Items.Select(i => i.UserId)
                .Order()
                .ToArray()
                .ShouldBe(
                    new int?[] { users[0].Id, users[2].Id }
                        .Order()
                        .ToArray()
                );
        }
        else
        {
            dto.TotalCount.ShouldBe(TotalExpectedValues + memberships.Count);
        }
    }

    [Theory]
    [InlineData(UserRole.Super, false, true)]
    [InlineData(UserRole.Champion, false, false)]
    [InlineData(UserRole.Champion, true, true)]
    [InlineData(UserRole.Standard, false, false)]
    [InlineData(UserRole.Standard, true, false)]
    public async Task GetUsers_ReturnsNotAllowed_WhenExplicitlyRequestingUsersForAnOrganisationIsNotAllowedToAccess(
        UserRole userRole,
        bool requestsOwnOrganisation,
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
            CreateGetUsersQuery(
                organisationId: requestsOwnOrganisation ? userOrganisation : otherOrganisation
            ),
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
    public async Task GetUsers_WhenStandardUser_ReturnsNotAllowed()
    {
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                UserRole = UserRole.Standard,
                OrganisationId = 1,
            }
        );
        var result = await harness.Service.GetUsers(
            new GetUsersQueryDto(),
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<GetUsersError.NotAllowed>();
    }

    [Theory]
    [InlineData(UserRole.Super)]
    [InlineData(UserRole.Champion)]
    public async Task GetUsers_WhenChampionOrSuperUserShouldReturnAllActions(UserRole userRoles)
    {
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                UserRole = userRoles,
                OrganisationId = 1,
            }
        );
        var allActions = Enum.GetValues<UserMembershipAction>();
        var result = await harness.Service.GetUsers(
            _getAllUserQuery,
            TestContext.Current.CancellationToken
        );
        var users = result.ShouldBeSuccess().Items;
        var actions = users.SelectMany(x => x.Actions).Distinct();
        var intersection = allActions.Intersect(actions);
        intersection.Count().ShouldBe(allActions.Length);
    }

    [Fact]
    public async Task GetUsers_NoActionsAreShownAsPermittedForTheCurrentUser()
    {
        var membership = _faker.PickRandom(ViewableMemberships);
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = membership.User!.CognitoUsername,
            }
        );
        var result = await harness.Service.GetUsers(
            new GetUsersQueryDto() { Email = membership.User!.WorkEmail },
            TestContext.Current.CancellationToken
        );
        var users = result.ShouldBeSuccess().Items;
        var foundUser = users.First(x =>
            string.Equals(
                x.EmailAddress,
                membership.User.WorkEmail,
                StringComparison.OrdinalIgnoreCase
            )
        );
        foundUser.Actions.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(UserOrgStatus.Active, UserMembershipAction.EditUserRole, true)]
    [InlineData(UserOrgStatus.Inactive, UserMembershipAction.EditUserRole, true)]
    [InlineData(UserOrgStatus.Active, UserMembershipAction.DeactivateMembership, true)]
    [InlineData(UserOrgStatus.Active, UserMembershipAction.ReactivateMembership, false)]
    [InlineData(UserOrgStatus.Deactivated, UserMembershipAction.DeactivateMembership, false)]
    [InlineData(UserOrgStatus.RequestedAccess, UserMembershipAction.ApproveMembership, true)]
    [InlineData(UserOrgStatus.RequestedAccess, UserMembershipAction.RejectMembership, true)]
    [InlineData(UserOrgStatus.Deactivated, UserMembershipAction.ReactivateMembership, true)]
    public async Task GetUsers_UsersOfGivenState_MayHaveTheGivenAction(
        UserOrgStatus status,
        UserMembershipAction action,
        bool shouldContainAction
    )
    {
        var result = await _harness.Service.GetUsers(
            new() { Status = [status] },
            TestContext.Current.CancellationToken
        );
        var users = result.ShouldBeSuccess().Items;
        users.ShouldAllBe(x => x.Actions.Contains(action) == shouldContainAction);
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

    private IEnumerable<int> GetOrganisationIdsFromUserEmails(string emailAddress)
    {
        return (
            _seededUsers
                .FirstOrDefault(x =>
                    string.Equals(x.WorkEmail, emailAddress, StringComparison.OrdinalIgnoreCase)
                )
                ?.UserOrgMemberships!.Select(x => x.OrganisationId)
            ?? Enumerable.Empty<int>()
        )
            .Concat(
                _userRegistrationRequests
                    .Where(x =>
                        string.Equals(x.WorkEmail, emailAddress, StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(x => x.OrganisationId)
            )
            .Distinct();
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

    [Theory]
    [InlineData((int)UserOrgMembershipStatus.AwaitingSetup)]
    [InlineData((int)UserOrgMembershipStatus.Active)]
    [InlineData((int)UserOrgMembershipStatus.Inactive)]
    [InlineData((int)UserOrgMembershipStatus.Deactivated)]
    [InlineData((int)UserOrgMembershipStatus.Removed)]
    public async Task GetUserDetailsWithinOrganisation_MapsUserAndMembershipFields_WhenTheUserIsAMember(
        int status
    )
    {
        (User user, UserOrgMembership membership) = await AddUserWithMembership(
            status: (UserOrgMembershipStatus)status
        );

        GetUserInformationResult result = await Service.GetUserDetailsWithinOrganisation(
            user.Id,
            membership.OrganisationId,
            TestContext.Current.CancellationToken
        );

        UserInformationDto dto = result.ShouldBeSuccess();
        dto.ShouldBe(
            new UserInformationDto()
            {
                UserId = user.Id,
                FullName = user.FullName,
                WorkEmail = user.WorkEmail,
                WorkTelephone = user.WorkTelephone!,
                OrganisationMembershipId = membership.Id,
                OrganisationId = membership.OrganisationId,
                OrganisationName = membership.Organisation!.OrganisationName,
                UserRole = membership.UserRole,
                Status = (UserOrgStatus)membership.Status,
            }
        );
    }

    [Fact]
    public async Task GetUserDetailsWithinOrganisation_ReturnsEmptyTelephone_WhenTheUserHasNone()
    {
        (User user, UserOrgMembership membership) = await AddUserWithMembership(configureUser: x =>
            x.WorkTelephone = null
        );

        GetUserInformationResult result = await Service.GetUserDetailsWithinOrganisation(
            user.Id,
            membership.OrganisationId,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess().WorkTelephone.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUserDetailsWithinOrganisation_ReturnsTheRoleForTheRequestedOrganisation_WhenTheUserBelongsToSeveral()
    {
        User user = _userFaker.Generate();
        UserOrgMembership standardMembership = _userOrgMembershipFaker
            .Generate()
            .Update(x =>
            {
                x.User = user;
                x.Organisation = _organisationFaker.Generate();
                x.UserRole = UserRole.Standard;
            });
        UserOrgMembership championMembership = _userOrgMembershipFaker
            .Generate()
            .Update(x =>
            {
                x.User = user;
                x.Organisation = _organisationFaker.Generate();
                x.UserRole = UserRole.Champion;
            });
        await AddEntities(
            new[] { standardMembership, championMembership },
            TestContext.Current.CancellationToken
        );
        GetUserInformationResult standardResult = await Service.GetUserDetailsWithinOrganisation(
            user.Id,
            standardMembership.OrganisationId,
            TestContext.Current.CancellationToken
        );
        GetUserInformationResult championResult = await Service.GetUserDetailsWithinOrganisation(
            user.Id,
            championMembership.OrganisationId,
            TestContext.Current.CancellationToken
        );

        standardResult.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);
        championResult.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Champion);
    }

    [Fact]
    public async Task GetUserDetailsWithinOrganisation_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        GetUserInformationResult result = await Service.GetUserDetailsWithinOrganisation(
            userId: 1,
            organisationId: 99,
            TestContext.Current.CancellationToken
        );

        GetUsersError.OrganisationNotFound notFound = result
            .ShouldBeError()
            .ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUserDetailsWithinOrganisation_ReturnsUserNotFoundError_WhenTheUserIsNotAMemberOfTheOrganisation()
    {
        (User user, _) = await AddUserWithMembership();
        Organisation otherOrganisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        GetUserInformationResult result = await Service.GetUserDetailsWithinOrganisation(
            user.Id,
            otherOrganisation.Id,
            TestContext.Current.CancellationToken
        );

        GetUsersError.UserNotFound notFound = result
            .ShouldBeError()
            .ShouldBeOfType<GetUsersError.UserNotFound>();
        notFound.UserId.ShouldBe(user.Id);
        notFound.OrganisationId.ShouldBe(otherOrganisation.Id);
    }

    [Theory]
    [InlineData(UserRole.Super, true)]
    [InlineData(UserRole.Champion, false)]
    [InlineData(UserRole.Standard, false)]
    public async Task GetUserDetailsWithinOrganisation_ReturnsNotAllowedError_WhenTheOrganisationIsNotTheCallersOwn(
        UserRole callerRole,
        bool isAllowedToAccess
    )
    {
        (User user, UserOrgMembership membership) = await AddUserWithMembership();
        IUserService service = new ServiceTestHarness<IUserService>(Context)
            .UpdateCurrentUser(x =>
                x with
                {
                    UserRole = callerRole,
                    OrganisationId = membership.OrganisationId + 1,
                }
            )
            .Service;

        GetUserInformationResult result = await service.GetUserDetailsWithinOrganisation(
            user.Id,
            membership.OrganisationId,
            TestContext.Current.CancellationToken
        );

        if (isAllowedToAccess)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result.ShouldBeError().ShouldBeOfType<GetUsersError.NotAllowed>();
        }
    }

    [Theory]
    [InlineData(UserRole.Super, true)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Standard, false)]
    public async Task GetUserDetailsWithinOrganisation_OnlyAllowsChampionsAndSupers_WhenTheOrganisationIsTheCallersOwn(
        UserRole callerRole,
        bool isAllowedToAccess
    )
    {
        (User user, UserOrgMembership membership) = await AddUserWithMembership();
        IUserService service = new ServiceTestHarness<IUserService>(Context)
            .UpdateCurrentUser(x =>
                x with
                {
                    UserRole = callerRole,
                    OrganisationId = membership.OrganisationId,
                }
            )
            .Service;

        GetUserInformationResult result = await service.GetUserDetailsWithinOrganisation(
            user.Id,
            membership.OrganisationId,
            TestContext.Current.CancellationToken
        );

        if (isAllowedToAccess)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result.ShouldBeError().ShouldBeOfType<GetUsersError.NotAllowed>();
        }
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_ShouldUpdateCurrentOrganisationInTheDatabase()
    {
        (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
            3,
            organisationStatus: UserOrgStatus.Active
        );
        _harness.UpdateCurrentUser(user);

        var selectedMembership = _faker.PickRandom(memberships);

        var result = await _harness.Service.UpdateCurrentOrganisation(
            new UpdateCurrentOrganisationCommand()
            {
                OrganisationId = selectedMembership.OrganisationId,
            },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess();

        var updatedUser = await _harness
            .GetClearedContext()
            .Users.Include(x => x.UserOrgMemberships)
            .FirstOrDefaultAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken);

        updatedUser
            ?.FindCurrentOrganisationId()
            .ShouldNotBeNull()
            .ShouldBe(selectedMembership.OrganisationId);
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenProvidingOrganisationThatIsNotOneThatBelongsToUser_ShouldReturnError()
    {
        var otherOrganisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        (User user, _) = await AddUserWithManyMemberships(
            3,
            organisationStatus: UserOrgStatus.Active
        );
        _harness.UpdateCurrentUser(user);

        Result<UpdateCurrentOrganisationError> result =
            await _harness.Service.UpdateCurrentOrganisation(
                new UpdateCurrentOrganisationCommand() { OrganisationId = otherOrganisation.Id },
                TestContext.Current.CancellationToken
            );

        result
            .ShouldBeError()
            .ShouldBeOfType<UpdateCurrentOrganisationError.ProvidedOrganisationWasNotValid>();
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_ShouldBeAbleToBeRunMultipleTimes()
    {
        (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
            3,
            organisationStatus: UserOrgStatus.Active
        );
        _harness.UpdateCurrentUser(user);

        UserOrgMembership selectedMembership = null!;
        foreach (var _ in Enumerable.Range(0, 5))
        {
            selectedMembership = _faker.PickRandom(memberships);

            var result = await _harness.Service.UpdateCurrentOrganisation(
                new UpdateCurrentOrganisationCommand()
                {
                    OrganisationId = selectedMembership.OrganisationId,
                },
                TestContext.Current.CancellationToken
            );

            result.ShouldBeSuccess();
        }

        var updatedUser = await _harness
            .GetClearedContext()
            .Users.Include(x => x.UserOrgMemberships)
            .FirstOrDefaultAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken);

        updatedUser
            ?.FindCurrentOrganisationId()
            .ShouldNotBeNull()
            .ShouldBe(selectedMembership.OrganisationId);
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenMembershipIsNotAuthorised_ShouldReturnErrorAndKeepPreviousSelection()
    {
        UserOrgMembershipStatus[] unauthorisedStatuses =
        [
            UserOrgMembershipStatus.AwaitingSetup,
            UserOrgMembershipStatus.Deactivated,
        ];

        foreach (UserOrgMembershipStatus unauthorisedStatus in unauthorisedStatuses)
        {
            (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
                2,
                organisationStatus: UserOrgStatus.Active
            );
            UserOrgMembership unauthorisedMembership = new UserOrgMembershipFaker()
                .RuleFor(x => x.Status, _ => unauthorisedStatus)
                .Generate()
                .Update(x =>
                {
                    x.UserId = user.Id;
                    x.Organisation = _organisationFaker.Generate();
                });
            await AddEntity(unauthorisedMembership, TestContext.Current.CancellationToken);
            _harness.UpdateCurrentUser(user);

            UserOrgMembership previousSelection = memberships[0];
            (
                await _harness.Service.UpdateCurrentOrganisation(
                    new UpdateCurrentOrganisationCommand
                    {
                        OrganisationId = previousSelection.OrganisationId,
                    },
                    TestContext.Current.CancellationToken
                )
            ).ShouldBeSuccess();

            Result<UpdateCurrentOrganisationError> result =
                await _harness.Service.UpdateCurrentOrganisation(
                    new UpdateCurrentOrganisationCommand
                    {
                        OrganisationId = unauthorisedMembership.OrganisationId,
                    },
                    TestContext.Current.CancellationToken
                );

            result
                .ShouldBeError()
                .ShouldBeOfType<UpdateCurrentOrganisationError.ProvidedOrganisationWasNotValid>();

            var updatedUser = await _harness
                .GetClearedContext()
                .Users.Include(x => x.UserOrgMemberships)
                .SingleAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken);

            updatedUser.FindCurrentOrganisationId().ShouldBe(previousSelection.OrganisationId);
        }
    }

    [Fact]
    public async Task UpdateCurrentOrganisation_WhenOrganisationIsNotActive_ShouldReturnErrorAndKeepPreviousSelection()
    {
        UserOrgStatus[] nonActiveStatuses = Enum.GetValues<UserOrgStatus>()
            .Except([UserOrgStatus.Active])
            .ToArray();

        foreach (UserOrgStatus nonActiveStatus in nonActiveStatuses)
        {
            (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
                2,
                organisationStatus: UserOrgStatus.Active
            );
            UserOrgMembership inactiveOrganisationMembership = new UserOrgMembershipFaker()
                .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active)
                .Generate()
                .Update(x =>
                {
                    x.UserId = user.Id;
                    x.Organisation = _organisationFaker
                        .Generate()
                        .Update(o => o.Status = nonActiveStatus);
                });
            await AddEntity(inactiveOrganisationMembership, TestContext.Current.CancellationToken);
            _harness.UpdateCurrentUser(user);

            UserOrgMembership previousSelection = memberships[0];
            (
                await _harness.Service.UpdateCurrentOrganisation(
                    new UpdateCurrentOrganisationCommand
                    {
                        OrganisationId = previousSelection.OrganisationId,
                    },
                    TestContext.Current.CancellationToken
                )
            ).ShouldBeSuccess();

            Result<UpdateCurrentOrganisationError> result =
                await _harness.Service.UpdateCurrentOrganisation(
                    new UpdateCurrentOrganisationCommand
                    {
                        OrganisationId = inactiveOrganisationMembership.OrganisationId,
                    },
                    TestContext.Current.CancellationToken
                );

            result
                .ShouldBeError()
                .ShouldBeOfType<UpdateCurrentOrganisationError.ProvidedOrganisationWasNotValid>();

            var updatedUser = await _harness
                .GetClearedContext()
                .Users.Include(x => x.UserOrgMemberships)
                .SingleAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken);

            updatedUser.FindCurrentOrganisationId().ShouldBe(previousSelection.OrganisationId);
        }
    }

    [Fact]
    public async Task GetCurrentUserOrganisations_ShouldReturnOnlyTheOrganisationsOfTheCurrentUsersMemberships()
    {
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
                3,
                organisationStatus: UserOrgStatus.Active
            );
            _harness.UpdateCurrentUser(user).UpdateCurrentUser(x => x with { UserRole = role });

            IReadOnlyCollection<OrganisationListDto> result =
                await _harness.Service.GetCurrentUserOrganisations(
                    TestContext.Current.CancellationToken
                );

            result
                .Select(x => x.Id)
                .ShouldBe(memberships.Select(x => x.OrganisationId), ignoreOrder: true);
            result
                .Select(x => x.OrganisationName)
                .ShouldBe(
                    memberships
                        .Select(x => x.Organisation!.OrganisationName)
                        .Order(StringComparer.OrdinalIgnoreCase)
                );
        }
    }

    [Fact]
    public async Task GetCurrentUserOrganisations_ShouldExcludeUnauthorisedMembershipsAndInactiveOrganisations()
    {
        (User user, UserOrgMembership[] validMemberships) = await AddUserWithManyMemberships(
            1,
            organisationStatus: UserOrgStatus.Active
        );
        UserOrgMembership deactivatedMembership = new UserOrgMembershipFaker()
            .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Deactivated)
            .Generate()
            .Update(x =>
            {
                x.UserId = user.Id;
                x.Organisation = _organisationFaker
                    .Generate()
                    .Update(o => o.Status = UserOrgStatus.Active);
            });
        UserOrgMembership inactiveOrganisationMembership = new UserOrgMembershipFaker()
            .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active)
            .Generate()
            .Update(x =>
            {
                x.UserId = user.Id;
                x.Organisation = _organisationFaker
                    .Generate()
                    .Update(o => o.Status = UserOrgStatus.Deactivated);
            });
        await AddEntities(
            [deactivatedMembership, inactiveOrganisationMembership],
            TestContext.Current.CancellationToken
        );
        _harness.UpdateCurrentUser(user);

        IReadOnlyCollection<OrganisationListDto> result =
            await _harness.Service.GetCurrentUserOrganisations(
                TestContext.Current.CancellationToken
            );

        result.Select(x => x.Id).ShouldBe([validMemberships.Single().OrganisationId]);
    }

    private async Task<(User User, UserOrgMembership Membership)> AddUserWithMembership(
        Action<User>? configureUser = null,
        UserOrgMembershipStatus status = UserOrgMembershipStatus.Active
    )
    {
        (User user, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(
            1,
            configureUser,
            status
        );
        return (user, memberships.Single());
    }

    private async Task<(User User, UserOrgMembership[] Memberships)> AddUserWithManyMemberships(
        int numberOfMemberships,
        Action<User>? configureUser = null,
        UserOrgMembershipStatus status = UserOrgMembershipStatus.Active,
        UserOrgStatus? organisationStatus = null
    )
    {
        User user = _userFaker.Generate().Update(x => configureUser?.Invoke(x));
        UserOrgMembership[] memberships = new UserOrgMembershipFaker()
            .RuleFor(x => x.Status, _ => status)
            .RuleFor(x => x.UserRole, _ => UserRole.Standard)
            .Generate(numberOfMemberships)
            .Select(x =>
                x.Update(x =>
                {
                    x.User = user;
                    x.Organisation = _organisationFaker.Generate();
                    if (organisationStatus is { } orgStatus)
                    {
                        x.Organisation.Status = orgStatus;
                    }
                })
            )
            .ToArray();

        await AddEntities(memberships, TestContext.Current.CancellationToken);
        return (user, memberships);
    }

    private static GetUsersQueryDto CreateGetUsersQuery(
        int? organisationId = 1,
        int page = 1,
        int pageSize = 100,
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

    private async Task<User> AddCallerUser()
    {
        User caller = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        _harness = _harness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = caller.CognitoUsername,
                Email = caller.WorkEmail,
                UserRole = UserRole.Super,
            }
        );
        return caller;
    }

    [Fact]
    public async Task RemoveUser_ShouldAnonymiseTheUsersPersonalDetails()
    {
        await AddCallerUser();
        DateTime lastActive = _currentDateTime.AddDays(-1);
        User target = _userFaker.Generate();
        target.LastActive = lastActive;
        await AddEntity(target, TestContext.Current.CancellationToken);

        _ = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        User? databaseUser = await Context.Users.FindAsync(
            [target.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        databaseUser.Title.ShouldBe("REMOVED");
        databaseUser.FullName.ShouldBe($"User-{target.Id}");
        databaseUser.JobTitle.ShouldBe("REMOVED");
        databaseUser.WorkTelephone.ShouldBe("REMOVED");
        databaseUser.WorkEmail.ShouldBe($"removed-user-{target.Id}@removed.invalid");
        databaseUser.LastActive.ShouldBe(lastActive);
        databaseUser.UpdatedAt.ShouldBe(_currentDateTime);
    }

    [Fact]
    public async Task RemoveUser_ShouldAnonymiseOnlyTheLinkedApprovedRegistrationRequest()
    {
        User caller = await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        User other = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        UserRegistrationRequest request = await AddApprovedRegistrationRequest(target, caller);
        UserRegistrationRequest otherRequest = await AddApprovedRegistrationRequest(other, caller);
        string otherName = otherRequest.FullName;
        string otherEmail = otherRequest.WorkEmail;
        string otherPhone = otherRequest.PhoneNumber;

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess();
        UserRegistrationRequest databaseRequest = await Context
            .UserRegistrationRequests.AsNoTracking()
            .SingleAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);
        databaseRequest.FullName.ShouldBe($"User-{target.Id}");
        databaseRequest.WorkEmail.ShouldBe($"removed-user-{target.Id}@removed.invalid");
        databaseRequest.PhoneNumber.ShouldBe("REMOVED");
        databaseRequest.RequestGuid.ShouldBe(request.RequestGuid);
        databaseRequest.OrganisationId.ShouldBe(request.OrganisationId);
        databaseRequest.CreatedUserId.ShouldBe(target.Id);
        databaseRequest.CreatedAt.ShouldBe(request.CreatedAt);
        databaseRequest.ApprovedByUserId.ShouldBe(caller.Id);
        databaseRequest.ApprovedAt.ShouldBe(request.ApprovedAt);
        databaseRequest.RejectedBy.ShouldBeNull();
        databaseRequest.RejectedAt.ShouldBeNull();
        databaseRequest.GetState().ShouldBe(UserRegistrationRequest.State.Approved);

        UserRegistrationRequest unchangedRequest = await Context
            .UserRegistrationRequests.AsNoTracking()
            .SingleAsync(x => x.Id == otherRequest.Id, TestContext.Current.CancellationToken);
        unchangedRequest.FullName.ShouldBe(otherName);
        unchangedRequest.WorkEmail.ShouldBe(otherEmail);
        unchangedRequest.PhoneNumber.ShouldBe(otherPhone);
    }

    [Fact]
    public async Task RemoveUser_ShouldAnonymiseOnlyMatchingOnboardingCreatorsAndPreserveMetadata()
    {
        User caller = await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        UserOnboardingRecord pendingRecord = await AddOnboardingRecordCreatedBy(target.WorkEmail);
        UserOnboardingRecord consumedRecord = await AddOnboardingRecordCreatedBy(
            target.WorkEmail,
            consumed: true
        );
        UserOnboardingRecord unrelatedRecord = await AddOnboardingRecordCreatedBy(caller.WorkEmail);

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess();
        foreach (UserOnboardingRecord originalRecord in new[] { pendingRecord, consumedRecord })
        {
            UserOnboardingRecord databaseRecord = await Context
                .UserOnboardingRecords.AsNoTracking()
                .SingleAsync(
                    x => x.SetupToken == originalRecord.SetupToken,
                    TestContext.Current.CancellationToken
                );
            databaseRecord.CreatedBy.ShouldBe($"removed-user-{target.Id}@removed.invalid");
            databaseRecord.UserId.ShouldBe(originalRecord.UserId);
            databaseRecord.UserId.ShouldNotBe(target.Id);
            databaseRecord.CorrelationId.ShouldBe(originalRecord.CorrelationId);
            databaseRecord.CreatedAt.ShouldBe(originalRecord.CreatedAt);
            databaseRecord.ConsumedAt.ShouldBe(originalRecord.ConsumedAt);
            databaseRecord.ResendCount.ShouldBe(originalRecord.ResendCount);
        }

        UserOnboardingRecord unchangedRecord = await Context
            .UserOnboardingRecords.AsNoTracking()
            .SingleAsync(
                x => x.SetupToken == unrelatedRecord.SetupToken,
                TestContext.Current.CancellationToken
            );
        unchangedRecord.CreatedBy.ShouldBe(caller.WorkEmail);
    }

    private async Task<UserOnboardingRecord> AddOnboardingRecordCreatedBy(
        string createdBy,
        bool consumed = false
    )
    {
        User owner = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        var record = new UserOnboardingRecord
        {
            SetupToken = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = _currentDateTime.AddDays(-1),
            CreatedBy = createdBy,
            ResendCount = 2,
            UserId = owner.Id,
        };
        if (consumed)
        {
            record.MarkAsConsumed(_currentDateTime);
        }

        return await AddEntity(record, TestContext.Current.CancellationToken);
    }

    private async Task<UserRegistrationRequest> AddApprovedRegistrationRequest(
        User user,
        User approver
    )
    {
        var organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        UserRegistrationRequest request = new UserRegistrationRequestFaker()
            .RuleFor(x => x.OrganisationId, _ => organisation.Id)
            .RuleFor(x => x.CreatedUserId, _ => user.Id)
            .RuleFor(x => x.CreatedAt, _ => _currentDateTime.AddDays(-1))
            .Generate();
        request.Approve(approver, _currentDateTime);
        return await AddEntity(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RemoveUser_ShouldReturnTheAnonymisedDisplayName()
    {
        await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        RemovedUserDto dto = result.ShouldBeSuccess();
        dto.DisplayName.ShouldBe($"User-{target.Id}");
    }

    [Fact]
    public async Task RemoveUser_ShouldRecordTheRemovalInTheAuditTrail()
    {
        User caller = await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);

        _ = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        UserAudit audit = await Context.UserAudits.SingleAsync(
            a => a.UserId == target.Id,
            TestContext.Current.CancellationToken
        );
        audit.EventType.ShouldBe(IamEventType.Deleted);
        audit.UpdatedBy.ShouldBe(caller.Id);
        audit.UpdatedAt.ShouldBe(_currentDateTime);
    }

    [Theory]
    [InlineData(UserAuditFieldPaths.Title)]
    [InlineData(UserAuditFieldPaths.FullName)]
    [InlineData(UserAuditFieldPaths.JobTitle)]
    [InlineData(UserAuditFieldPaths.WorkTelephone)]
    [InlineData(UserAuditFieldPaths.WorkEmail)]
    public async Task RemoveUser_ShouldClearOnlyTheRemovedUsersPersonalAuditValues(string fieldPath)
    {
        await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        User other = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        UserAudit targetAudit = new()
        {
            UserId = target.Id,
            FieldPath = fieldPath,
            OldValue = "old personal value",
            NewValue = "new personal value",
            EventType = IamEventType.FieldUpdated,
            UpdatedAt = _currentDateTime,
        };
        UserAudit otherAudit = new()
        {
            UserId = other.Id,
            FieldPath = fieldPath,
            OldValue = "old personal value",
            NewValue = "new personal value",
            EventType = IamEventType.FieldUpdated,
            UpdatedAt = _currentDateTime,
        };
        UserAudit nonPersonalAudit = new()
        {
            UserId = target.Id,
            FieldPath = nameof(User.UserType),
            OldValue = "old type",
            NewValue = "new type",
            EventType = IamEventType.FieldUpdated,
            UpdatedAt = _currentDateTime,
        };
        await AddEntities(
            [targetAudit, otherAudit, nonPersonalAudit],
            TestContext.Current.CancellationToken
        );

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess();
        UserAudit updatedTargetAudit = await Context
            .UserAudits.AsNoTracking()
            .SingleAsync(a => a.Id == targetAudit.Id, TestContext.Current.CancellationToken);
        UserAudit unchangedOtherAudit = await Context
            .UserAudits.AsNoTracking()
            .SingleAsync(a => a.Id == otherAudit.Id, TestContext.Current.CancellationToken);
        UserAudit unchangedNonPersonalAudit = await Context
            .UserAudits.AsNoTracking()
            .SingleAsync(a => a.Id == nonPersonalAudit.Id, TestContext.Current.CancellationToken);

        updatedTargetAudit.OldValue.ShouldBeNull();
        updatedTargetAudit.NewValue.ShouldBeNull();
        unchangedOtherAudit.OldValue.ShouldBe("old personal value");
        unchangedOtherAudit.NewValue.ShouldBe("new personal value");
        unchangedNonPersonalAudit.OldValue.ShouldBe("old type");
        unchangedNonPersonalAudit.NewValue.ShouldBe("new type");
    }

    [Fact]
    public async Task RemoveUser_WhenUserDoesNotExist_ShouldReturnUserNotFoundError()
    {
        var result = await Service.RemoveUser(999, TestContext.Current.CancellationToken);

        result.ShouldBeError().ShouldBeOfType<RemoveUserError.UserNotFound>();
    }

    [Fact]
    public async Task RemoveUser_WhenCallerAttemptsToRemoveThemselves_ShouldReturnCannotRemoveSelfError()
    {
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        _harness = _harness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = target.CognitoUsername,
            }
        );

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeError().ShouldBeOfType<RemoveUserError.CannotRemoveSelf>();

        User? databaseUser = await Context.Users.FindAsync(
            [target.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        databaseUser.FullName.ShouldBe(target.FullName);
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    public async Task RemoveUser_WhenCallerIsNotSuper_ShouldReturnNotAllowedError(
        UserRole callerRole
    )
    {
        _harness = _harness.UpdateCurrentUser(x => x with { UserRole = callerRole });

        var result = await Service.RemoveUser(1, TestContext.Current.CancellationToken);

        result.ShouldBeError().ShouldBeOfType<RemoveUserError.NotAllowed>();
    }

    [Fact]
    public async Task RemoveUser_ShouldDeleteTheUsersCognitoAccount()
    {
        await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        AddCognitoAccount(target);

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess();
        _harness.Cognito.GetUser(target.CognitoUsername).ShouldBeNull();
    }

    [Fact]
    public async Task RemoveUser_WhenUserHasNoCognitoAccount_ShouldStillRemoveTheUser()
    {
        await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess().DisplayName.ShouldBe($"User-{target.Id}");
    }

    [Fact]
    public async Task RemoveUser_ShouldTransitionTheUsersMembershipsToRemoved()
    {
        await AddCallerUser();
        (User target, UserOrgMembership membership) = await AddUserWithMembership();

        _ = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        UserOrgMembership? databaseMembership = await Context.UserOrgMemberships.FindAsync(
            [membership.Id],
            TestContext.Current.CancellationToken
        );
        databaseMembership.ShouldNotBeNull();
        databaseMembership.Status.ShouldBe(UserOrgMembershipStatus.Removed);
    }

    [Fact]
    public async Task RemoveUser_ShouldRemoveMembershipsAcrossAllOrganisations()
    {
        await AddCallerUser();
        (User target, UserOrgMembership[] memberships) = await AddUserWithManyMemberships(3);
        AddCognitoAccount(target);

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess();
        List<UserOrgMembership> databaseMemberships = await Context
            .UserOrgMemberships.AsNoTracking()
            .Where(m => m.UserId == target.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        databaseMemberships.Count.ShouldBe(memberships.Length);
        databaseMemberships.ShouldAllBe(m => m.Status == UserOrgMembershipStatus.Removed);
        _harness.Cognito.GetUser(target.CognitoUsername).ShouldBeNull();
    }

    [Fact]
    public async Task RemoveUser_WhenCalledAgainAfterASuccessfulRemoval_ShouldReturnNotAllowedInCurrentStateError()
    {
        await AddCallerUser();
        (User target, _) = await AddUserWithMembership();
        AddCognitoAccount(target);

        _ = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);
        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        var error = result
            .ShouldBeError()
            .ShouldBeOfType<RemoveUserError.NotAllowedInCurrentState>();
        error.TransitionResult.CurrentState.ShouldBe(UserOrgStatus.Removed);
    }

    [Fact]
    public async Task RemoveUser_WhenCognitoDeleteFails_ShouldRollBackTheAnonymisation()
    {
        User caller = await AddCallerUser();
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        UserRegistrationRequest request = await AddApprovedRegistrationRequest(target, caller);
        string originalName = request.FullName;
        string originalEmail = request.WorkEmail;
        string originalPhone = request.PhoneNumber;
        UserOnboardingRecord onboardingRecord = await AddOnboardingRecordCreatedBy(
            target.WorkEmail
        );
        _harness
            .Cognito.Mock.WhenForAnyArgs(x => x.AdminDeleteUserAsync(default!, default!))
            .Throws(new TooManyRequestsException("Rate exceeded"));

        await Should.ThrowAsync<TooManyRequestsException>(() =>
            Service.RemoveUser(target.Id, TestContext.Current.CancellationToken)
        );

        User databaseUser = await Context
            .Users.AsNoTracking()
            .SingleAsync(u => u.Id == target.Id, TestContext.Current.CancellationToken);
        databaseUser.FullName.ShouldBe(target.FullName);
        databaseUser.WorkEmail.ShouldBe(target.WorkEmail);

        UserRegistrationRequest databaseRequest = await Context
            .UserRegistrationRequests.AsNoTracking()
            .SingleAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);
        databaseRequest.FullName.ShouldBe(originalName);
        databaseRequest.WorkEmail.ShouldBe(originalEmail);
        databaseRequest.PhoneNumber.ShouldBe(originalPhone);

        UserOnboardingRecord databaseOnboardingRecord = await Context
            .UserOnboardingRecords.AsNoTracking()
            .SingleAsync(
                x => x.SetupToken == onboardingRecord.SetupToken,
                TestContext.Current.CancellationToken
            );
        databaseOnboardingRecord.CreatedBy.ShouldBe(onboardingRecord.CreatedBy);

        bool hasRemovalAudit = await Context.UserAudits.AnyAsync(
            a => a.UserId == target.Id && a.EventType == IamEventType.Deleted,
            TestContext.Current.CancellationToken
        );
        hasRemovalAudit.ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveUser_WhenCommitFailsAfterCognitoDeletion_ShouldRollBackAndAllowRetry()
    {
        User caller = await AddCallerUser();
        (User target, UserOrgMembership membership) = await AddUserWithMembership();
        UserRegistrationRequest request = await AddApprovedRegistrationRequest(target, caller);
        UserOnboardingRecord onboardingRecord = await AddOnboardingRecordCreatedBy(
            target.WorkEmail
        );
        UserAudit personalAudit = await AddEntity(
            new UserAudit
            {
                UserId = target.Id,
                FieldPath = UserAuditFieldPaths.FullName,
                OldValue = "old personal value",
                NewValue = target.FullName,
                EventType = IamEventType.FieldUpdated,
                UpdatedAt = _currentDateTime,
            },
            TestContext.Current.CancellationToken
        );

        var interceptor = new FailFirstCommitInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                Context.Database.GetConnectionString(),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ukps")
            )
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(interceptor)
            .Options;
        await using var failureContext = new AppDbContext(options);
        _harness = new ServiceTestHarness<IUserService>(failureContext)
            .UpdateCurrentTime(_currentDateTime)
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = caller.CognitoUsername,
                    Email = caller.WorkEmail,
                    UserRole = UserRole.Super,
                }
            );
        AddCognitoAccount(target);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Service.RemoveUser(target.Id, TestContext.Current.CancellationToken)
        );

        _harness.Cognito.GetUser(target.CognitoUsername).ShouldBeNull();
        User unchangedUser = await Context
            .Users.AsNoTracking()
            .SingleAsync(u => u.Id == target.Id, TestContext.Current.CancellationToken);
        unchangedUser.FullName.ShouldBe(target.FullName);
        unchangedUser.WorkEmail.ShouldBe(target.WorkEmail);
        UserRegistrationRequest unchangedRequest = await Context
            .UserRegistrationRequests.AsNoTracking()
            .SingleAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);
        unchangedRequest.FullName.ShouldBe(request.FullName);
        unchangedRequest.WorkEmail.ShouldBe(request.WorkEmail);
        unchangedRequest.PhoneNumber.ShouldBe(request.PhoneNumber);
        UserOnboardingRecord unchangedOnboardingRecord = await Context
            .UserOnboardingRecords.AsNoTracking()
            .SingleAsync(
                x => x.SetupToken == onboardingRecord.SetupToken,
                TestContext.Current.CancellationToken
            );
        unchangedOnboardingRecord.CreatedBy.ShouldBe(onboardingRecord.CreatedBy);
        var unchangedMembership = await Context
            .UserOrgMemberships.AsNoTracking()
            .SingleAsync(m => m.Id == membership.Id, TestContext.Current.CancellationToken);
        unchangedMembership.Status.ShouldBe(UserOrgMembershipStatus.Active);
        var unchangedAudit = await Context
            .UserAudits.AsNoTracking()
            .SingleAsync(a => a.Id == personalAudit.Id, TestContext.Current.CancellationToken);
        unchangedAudit.OldValue.ShouldBe("old personal value");
        unchangedAudit.NewValue.ShouldBe(target.FullName);
        (
            await Context.UserAudits.AnyAsync(
                a => a.UserId == target.Id && a.EventType == IamEventType.Deleted,
                TestContext.Current.CancellationToken
            )
        ).ShouldBeFalse();

        var result = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSuccess().DisplayName.ShouldBe($"User-{target.Id}");
        interceptor.CommitAttempts.ShouldBe(2);
        var removedUser = await Context
            .Users.AsNoTracking()
            .SingleAsync(u => u.Id == target.Id, TestContext.Current.CancellationToken);
        removedUser.WorkEmail.ShouldBe($"removed-user-{target.Id}@removed.invalid");
        UserRegistrationRequest anonymisedRequest = await Context
            .UserRegistrationRequests.AsNoTracking()
            .SingleAsync(x => x.Id == request.Id, TestContext.Current.CancellationToken);
        anonymisedRequest.FullName.ShouldBe($"User-{target.Id}");
        anonymisedRequest.WorkEmail.ShouldBe($"removed-user-{target.Id}@removed.invalid");
        anonymisedRequest.PhoneNumber.ShouldBe("REMOVED");
        UserOnboardingRecord anonymisedOnboardingRecord = await Context
            .UserOnboardingRecords.AsNoTracking()
            .SingleAsync(
                x => x.SetupToken == onboardingRecord.SetupToken,
                TestContext.Current.CancellationToken
            );
        anonymisedOnboardingRecord.CreatedBy.ShouldBe($"removed-user-{target.Id}@removed.invalid");
        var removedMembership = await Context
            .UserOrgMemberships.AsNoTracking()
            .SingleAsync(m => m.Id == membership.Id, TestContext.Current.CancellationToken);
        removedMembership.Status.ShouldBe(UserOrgMembershipStatus.Removed);
        var clearedAudit = await Context
            .UserAudits.AsNoTracking()
            .SingleAsync(a => a.Id == personalAudit.Id, TestContext.Current.CancellationToken);
        clearedAudit.OldValue.ShouldBeNull();
        clearedAudit.NewValue.ShouldBeNull();
        (
            await Context.UserAudits.CountAsync(
                a => a.UserId == target.Id && a.EventType == IamEventType.Deleted,
                TestContext.Current.CancellationToken
            )
        ).ShouldBe(1);
    }

    private sealed class FailFirstCommitInterceptor : DbTransactionInterceptor
    {
        public int CommitAttempts { get; private set; }

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default
        )
        {
            CommitAttempts++;
            if (CommitAttempts == 1)
            {
                throw new InvalidOperationException("Simulated database commit failure.");
            }

            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    public async Task RemoveUser_WhenCallerAttemptsToRemoveThemselves_ShouldNotDeleteTheirCognitoAccount()
    {
        User target = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        AddCognitoAccount(target);
        _harness = _harness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = target.CognitoUsername,
            }
        );

        _ = await Service.RemoveUser(target.Id, TestContext.Current.CancellationToken);

        _harness.Cognito.GetUser(target.CognitoUsername).ShouldNotBeNull();
    }

    private void AddCognitoAccount(User user)
    {
        _harness.Cognito.AddCurrentUser(
            _mockUserFaker.Generate() with
            {
                Username = user.CognitoUsername.Value,
            }
        );
    }
}
