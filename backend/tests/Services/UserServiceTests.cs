using Shouldly;
using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
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
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
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
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        Context.Users.Add(EntityFactory.CreateUser(id: 10, workEmail: "user@example.com"));
        Context.UserOrgMemberships.Add(
            EntityFactory.CreateMembership(
                id: 100,
                userId: 10,
                organisationId: 1,
                role: UserRole.Champion,
                status: UserOrgStatus.Active
            )
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(10);
        item.EmailAddress.ShouldBe("user@example.com");
        item.Role.ShouldBe(UserRole.Champion);
        item.Status.ShouldBe(UserOrgStatus.Active);
        item.LastActive.ShouldBeNull();
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 1, workEmail: "requested-access@example.com"),
            EntityFactory.CreateUser(id: 2, workEmail: "active@example.com"),
            EntityFactory.CreateUser(id: 3, workEmail: "inactive@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 1,
                organisationId: 1,
                status: UserOrgStatus.RequestedAccess
            ),
            EntityFactory.CreateMembership(
                id: 2,
                userId: 2,
                organisationId: 1,
                status: UserOrgStatus.Active
            ),
            EntityFactory.CreateMembership(
                id: 3,
                userId: 3,
                organisationId: 1,
                status: UserOrgStatus.Inactive
            )
        );
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
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 30, workEmail: "thirty@example.com"),
            EntityFactory.CreateUser(id: 10, workEmail: "ten@example.com"),
            EntityFactory.CreateUser(id: 20, workEmail: "twenty@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(id: 1, userId: 30, organisationId: 1),
            EntityFactory.CreateMembership(id: 2, userId: 10, organisationId: 1),
            EntityFactory.CreateMembership(id: 3, userId: 20, organisationId: 1)
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
            EntityFactory.CreateOrganisation(id: 1),
            EntityFactory.CreateOrganisation(id: 2)
        );
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 10, workEmail: "one@example.com"),
            EntityFactory.CreateUser(id: 20, workEmail: "two@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(id: 1, userId: 10, organisationId: 1),
            EntityFactory.CreateMembership(id: 2, userId: 20, organisationId: 2)
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
            EntityFactory.CreateOrganisation(id: 1),
            EntityFactory.CreateOrganisation(id: 2)
        );
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 10, workEmail: "active@example.com"),
            EntityFactory.CreateUser(id: 20, workEmail: "inactive@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 10,
                organisationId: 1,
                status: UserOrgStatus.Active
            ),
            EntityFactory.CreateMembership(
                id: 2,
                userId: 20,
                organisationId: 2,
                status: UserOrgStatus.Inactive
            )
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
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        Context.Users.Add(EntityFactory.CreateUser(id: 10, workEmail: "user@example.com"));
        Context.UserOrgMemberships.Add(
            EntityFactory.CreateMembership(id: 1, userId: 10, organisationId: 1)
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
            EntityFactory.CreateOrganisation(id: 1),
            EntityFactory.CreateOrganisation(id: 2)
        );
        Context.Users.Add(EntityFactory.CreateUser(id: 10, workEmail: "multi@example.com"));
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 10,
                organisationId: 1,
                allowedPharmaceuticalEntity: PharmaceuticalEntity.Medicines
            ),
            EntityFactory.CreateMembership(
                id: 2,
                userId: 10,
                organisationId: 2,
                allowedPharmaceuticalEntity: PharmaceuticalEntity.Medicines
            )
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
