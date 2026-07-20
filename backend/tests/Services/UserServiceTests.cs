using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests : DatabaseTestBase
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();
    private readonly IUserService _service;

    public UserServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _service = new ServiceTestHarness<IUserService>(Context).Service;
    }

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(99, 1, 20, [], TestContext.Current.CancellationToken);

        result.IsErr.ShouldBeTrue();
        GetUsersError.OrganisationNotFound notFound =
            result.Error.ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(_organisationFaker.Generate());
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(1, 1, 20, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
        dto.Page.ShouldBe(1);
        dto.PageSize.ShouldBe(20);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        var organisation = _organisationFaker.Generate();
        var user = _userFaker.Generate();
        user.Update(x => x.WorkEmail = "user@example.com");
        var membership = _userOrgMembershipFaker.Generate();
        membership.Update(x =>
        {
            x.UserId = user.Id;
            x.OrganisationId = organisation.Id;
            x.UserRole = UserRole.Champion;
            x.Status = UserOrgStatus.Active;
        });
        Context.Organisations.Add(organisation);
        Context.Users.Add(user);
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(1, 1, 20, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(user.Id);
        item.EmailAddress.ShouldBe(user.WorkEmail);
        item.Role.ShouldBe(UserRole.Champion);
        item.Status.ShouldBe(UserOrgStatus.Active);
        item.LastActive.ShouldBeNull();
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        UserOrgStatus[] userOrgStatuses =
        [
            UserOrgStatus.RequestedAccess,
            UserOrgStatus.Active,
            UserOrgStatus.Inactive,
        ];
        Organisation organisation = _organisationFaker.Generate();
        var data = userOrgStatuses.Select(s =>
        {
            var user = _userFaker.Generate();
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            membership.Status = s;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(
                1,
                1,
                20,
                [UserOrgStatus.Active, UserOrgStatus.Inactive],
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([2, 3]);
    }

    [Fact]
    public async Task GetUsers_PaginatesAndOrdersByUserId_WhenUsersExist()
    {
        Context.Organisations.Add(_organisationFaker.Generate().Update(x => x.Id = 1));
        Context.Users.AddRange(
            _userFaker.Generate().Update(x => x.Id = 30),
            _userFaker.Generate().Update(x => x.Id = 10),
            _userFaker.Generate().Update(x => x.Id = 20)
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 30;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 3;
                    x.UserId = 20;
                    x.OrganisationId = 1;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(1, 2, 1, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(3);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.AddRange(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "one@example.com";
                }),
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 20;
                    x.WorkEmail = "two@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 20;
                    x.OrganisationId = 2;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(null, 1, 20, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 20]);
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.AddRange(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "active@example.com";
                }),
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 20;
                    x.WorkEmail = "inactive@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                    x.Status = UserOrgStatus.Active;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 20;
                    x.OrganisationId = 2;
                    x.Status = UserOrgStatus.Inactive;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(
                null,
                1,
                20,
                [UserOrgStatus.Inactive],
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
        item.Status.ShouldBe(UserOrgStatus.Inactive);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        Context.Organisations.Add(_organisationFaker.Generate().Update(x => x.Id = 1));
        Context.Users.Add(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "user@example.com";
                })
        );
        Context.UserOrgMemberships.Add(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(1, 5, 20, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(1);
        dto.Page.ShouldBe(5);
    }

    [Fact]
    public async Task GetUsers_UserHasMembershipsInMultipleOrganisations_ReturnsOneRowPerMembership()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.Add(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "multi@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                    x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 10;
                    x.OrganisationId = 2;
                    x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await _service.GetUsers(null, 1, 20, [], TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 10]);
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
            null,
            1,
            20,
            [],
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
            dto.TotalCount.ShouldBe(3);
            dto.Items.Select(i => i.UserId)
                .ToArray()
                .ShouldBe([users[0].Id, users[1].Id, users[2].Id]);
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
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(
                otherOrganisation,
                1,
                20,
                [],
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
}
