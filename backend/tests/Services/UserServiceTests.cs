using Shouldly;
using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(99, 1, 20, []);

        result.IsErr.ShouldBeTrue();
        GetUsersError.OrganisationNotFound notFound =
            result.Error.ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(_organisationFaker.Generate());
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;

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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, [UserOrgStatus.Active, UserOrgStatus.Inactive]);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 2, 1, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, [UserOrgStatus.Inactive]);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 5, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
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
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 10]);
    }
}
