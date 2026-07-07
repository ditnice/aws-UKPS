using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;

namespace UKPS.Api.Tests.Services;

public class UserServiceTests
{
    private static AppDbContext CreateDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
        );

    [Fact]
    public async Task GetUsers_ReturnsNull_WhenOrganisationDoesNotExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(99, 1, 20, []);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(1, 1, 20, []);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        dbContext.Users.Add(CreateUser(id: 10, workEmail: "user@example.com"));
        dbContext.UserOrgMemberships.Add(
            CreateMembership(
                id: 100,
                userId: 10,
                organisationId: 1,
                role: UserRole.Champion,
                status: UserOrgStatus.Active
            )
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(1, 1, 20, []);

        Assert.NotNull(result);
        UserListItemDto item = Assert.Single(result.Items);
        Assert.Equal(10, item.UserId);
        Assert.Equal("user@example.com", item.EmailAddress);
        Assert.Equal(UserRole.Champion, item.Role);
        Assert.Equal(UserOrgStatus.Active, item.Status);
        Assert.Null(item.LastActive);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        dbContext.Users.AddRange(
            CreateUser(id: 1, workEmail: "requested-access@example.com"),
            CreateUser(id: 2, workEmail: "active@example.com"),
            CreateUser(id: 3, workEmail: "inactive@example.com")
        );
        dbContext.UserOrgMemberships.AddRange(
            CreateMembership(
                id: 1,
                userId: 1,
                organisationId: 1,
                status: UserOrgStatus.RequestedAccess
            ),
            CreateMembership(id: 2, userId: 2, organisationId: 1, status: UserOrgStatus.Active),
            CreateMembership(id: 3, userId: 3, organisationId: 1, status: UserOrgStatus.Inactive)
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(
            1,
            1,
            20,
            [UserOrgStatus.Active, UserOrgStatus.Inactive]
        );

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal([2, 3], result.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_PaginatesAndOrdersByUserId_WhenUsersExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        dbContext.Users.AddRange(
            CreateUser(id: 30, workEmail: "thirty@example.com"),
            CreateUser(id: 10, workEmail: "ten@example.com"),
            CreateUser(id: 20, workEmail: "twenty@example.com")
        );
        dbContext.UserOrgMemberships.AddRange(
            CreateMembership(id: 1, userId: 30, organisationId: 1),
            CreateMembership(id: 2, userId: 10, organisationId: 1),
            CreateMembership(id: 3, userId: 20, organisationId: 1)
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(1, 2, 1, []);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        UserListItemDto item = Assert.Single(result.Items);
        Assert.Equal(20, item.UserId);
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.AddRange(CreateOrganisation(id: 1), CreateOrganisation(id: 2));
        dbContext.Users.AddRange(
            CreateUser(id: 10, workEmail: "one@example.com"),
            CreateUser(id: 20, workEmail: "two@example.com")
        );
        dbContext.UserOrgMemberships.AddRange(
            CreateMembership(id: 1, userId: 10, organisationId: 1),
            CreateMembership(id: 2, userId: 20, organisationId: 2)
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(null, 1, 20, []);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal([10, 20], result.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.AddRange(CreateOrganisation(id: 1), CreateOrganisation(id: 2));
        dbContext.Users.AddRange(
            CreateUser(id: 10, workEmail: "active@example.com"),
            CreateUser(id: 20, workEmail: "inactive@example.com")
        );
        dbContext.UserOrgMemberships.AddRange(
            CreateMembership(id: 1, userId: 10, organisationId: 1, status: UserOrgStatus.Active),
            CreateMembership(id: 2, userId: 20, organisationId: 2, status: UserOrgStatus.Inactive)
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsers(
            null,
            1,
            20,
            [UserOrgStatus.Inactive]
        );

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        UserListItemDto item = Assert.Single(result.Items);
        Assert.Equal(20, item.UserId);
        Assert.Equal(UserOrgStatus.Inactive, item.Status);
    }

    private static Organisation CreateOrganisation(int id) =>
        new()
        {
            Id = id,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddress = "1 High Street\nLondon\nEC1A 1AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private static User CreateUser(int id, string workEmail) =>
        new()
        {
            Id = id,
            Username = workEmail,
            FirstName = "Test",
            LastName = "User",
            WorkEmail = workEmail,
        };

    private static UserOrgMembership CreateMembership(
        int id,
        int userId,
        int organisationId,
        UserRole role = UserRole.Standard,
        UserOrgStatus status = UserOrgStatus.Active
    ) =>
        new()
        {
            Id = id,
            UserId = userId,
            OrganisationId = organisationId,
            UserRole = role,
            Status = status,
        };
}
