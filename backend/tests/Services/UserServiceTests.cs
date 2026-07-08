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

        Assert.True(result.IsErr);
        GetUsersError.OrganisationNotFound notFound =
            Assert.IsType<GetUsersError.OrganisationNotFound>(result.Error);
        Assert.Equal(99, notFound.OrganisationId);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Empty(dto.Items);
        Assert.Equal(0, dto.TotalCount);
        Assert.Equal(1, dto.Page);
        Assert.Equal(20, dto.PageSize);
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(10, item.UserId);
        Assert.Equal("user@example.com", item.EmailAddress);
        Assert.Equal(UserRole.Champion, item.Role);
        Assert.Equal(UserOrgStatus.Active, item.Status);
        Assert.Null(item.LastActive);
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([2, 3], dto.Items.Select(i => i.UserId).ToArray());
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(3, dto.TotalCount);
        Assert.Equal(2, dto.Page);
        Assert.Equal(1, dto.PageSize);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(20, item.UserId);
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([10, 20], dto.Items.Select(i => i.UserId).ToArray());
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(1, dto.TotalCount);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(20, item.UserId);
        Assert.Equal(UserOrgStatus.Inactive, item.Status);
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Empty(dto.Items);
        Assert.Equal(1, dto.TotalCount);
        Assert.Equal(5, dto.Page);
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

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([10, 10], dto.Items.Select(i => i.UserId).ToArray());
    }
}
