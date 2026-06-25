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
    public async Task GetUsersByOrganisation_ReturnsNull_WhenOrganisationDoesNotExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsersByOrganisation(
            99,
            1,
            20,
            []
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUsersByOrganisation_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsersByOrganisation(
            1,
            1,
            20,
            []
        );

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetUsersByOrganisation_MapsUserMembershipFields_WhenUsersExist()
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
                status: UserOrgStatus.Approved
            )
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsersByOrganisation(
            1,
            1,
            20,
            []
        );

        Assert.NotNull(result);
        UserListItemDto item = Assert.Single(result.Items);
        Assert.Equal(10, item.UserId);
        Assert.Equal("user@example.com", item.EmailAddress);
        Assert.Equal(UserRole.Champion, item.Role);
        Assert.Equal(UserOrgStatus.Approved, item.Status);
        Assert.Null(item.LastActive);
    }

    [Fact]
    public async Task GetUsersByOrganisation_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        dbContext.Users.AddRange(
            CreateUser(id: 1, workEmail: "pending@example.com"),
            CreateUser(id: 2, workEmail: "approved@example.com"),
            CreateUser(id: 3, workEmail: "inactive@example.com")
        );
        dbContext.UserOrgMemberships.AddRange(
            CreateMembership(id: 1, userId: 1, organisationId: 1, status: UserOrgStatus.Pending),
            CreateMembership(id: 2, userId: 2, organisationId: 1, status: UserOrgStatus.Approved),
            CreateMembership(id: 3, userId: 3, organisationId: 1, status: UserOrgStatus.Inactive)
        );
        await dbContext.SaveChangesAsync();

        UserService service = new(dbContext);

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsersByOrganisation(
            1,
            1,
            20,
            [UserOrgStatus.Approved, UserOrgStatus.Inactive]
        );

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal([2, 3], result.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsersByOrganisation_PaginatesAndOrdersByUserId_WhenUsersExist()
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

        PaginatedResponseDto<UserListItemDto>? result = await service.GetUsersByOrganisation(
            1,
            2,
            1,
            []
        );

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        UserListItemDto item = Assert.Single(result.Items);
        Assert.Equal(20, item.UserId);
    }

    private static Organisation CreateOrganisation(int id) =>
        new()
        {
            Id = id,
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddressLine1 = "1 High Street",
            HeadOfficeTown = "London",
            HeadOfficePostcode = "EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
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
        UserOrgStatus status = UserOrgStatus.Approved
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
