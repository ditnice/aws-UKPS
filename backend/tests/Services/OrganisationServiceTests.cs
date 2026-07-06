using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Services;

public class OrganisationServiceTests
{
    private static AppDbContext CreateDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
        );

    [Fact]
    public async Task GetOrganisationById_OrganisationExists_ReturnsDto()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(
            new Organisation
            {
                Id = 1,
                OrganisationName = "Gov Pharma Ltd",
                OrganisationType = OrganisationType.PharmaCompany,
                AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
                HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
                HeadOfficeEmail = "info@pharma.gov.uk",
                HeadOfficeTelephone = "020 1234 5678",
                Status = UserOrgStatus.Approved,
                LastActive = new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc),
            }
        );
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Gov Pharma Ltd", result.OrganisationName);
        Assert.Equal(PharmaceuticalEntity.Medicines, result.AllowedPharmaceuticalEntity);
        Assert.Equal(OrganisationType.PharmaCompany, result.OrganisationType);
        Assert.Equal("10 Downing Street\nLondon\nSW1A 2AA", result.HeadOfficeAddress);
        Assert.Equal("info@pharma.gov.uk", result.HeadOfficeEmail);
        Assert.Equal("020 1234 5678", result.HeadOfficeTelephone);
        Assert.Equal(UserOrgStatus.Approved, result.Status);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), result.LastActive);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), result.CreatedAt);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNull()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation());
        await dbContext.SaveChangesAsync();
        int seededId = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(seededId + 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_UpdatesEditableFields()
    {
        await using AppDbContext dbContext = CreateDbContext();
        DateTime createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
        DateTime lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
        dbContext.Organisations.Add(CreateOrganisationForUpdate(createdAt, lastActive));
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.UpdateOrganisationDetails(
            id,
            CreateUpdateDto()
        );

        AssertUpdatedDetails(result, createdAt, lastActive);

        Organisation saved = await dbContext.Organisations.SingleAsync(o => o.Id == id);
        AssertUpdatedEntity(saved, createdAt, lastActive);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNull()
    {
        await using AppDbContext dbContext = CreateDbContext();
        OrganisationService service = new(dbContext);

        OrganisationDetailsDto? result = await service.UpdateOrganisationDetails(
            99,
            new UpdateOrganisationDetailsDto
            {
                OrganisationName = "New Pharma Ltd",
                HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
                HeadOfficeEmail = "new@example.com",
                HeadOfficeTelephone = "020 1111 1111",
            }
        );

        Assert.Null(result);
    }

    [Theory]
    [InlineData(UserRole.Champion)]
    [InlineData(UserRole.Super)]
    public async Task UpdateUserOrganisationMembershipRole_ShouldUpdatedUserMembershipRole(
        UserRole userRole
    )
    {
        await using AppDbContext dbContext = CreateDbContext();
        var userOrgMembership = await SetupUserOrgMembership(dbContext);
        var service = new OrganisationService(dbContext);
        var command = new UpdateUserOrganisationMembershipRoleCommandDto() { UserRole = userRole };
        UserOrganisationMembershipDto? result = await service.UpdateUserOrganisationMembershipRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.UserId,
            command,
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal(userRole, result.UserRole);
    }

    [Fact]
    public async Task UpdateUserOrganisationMembershipRole_OrganisationDoesNotExist_ShouldReturnNull()
    {
        await using AppDbContext dbContext = CreateDbContext();
        var userOrgMembership = await SetupUserOrgMembership(dbContext);
        var service = new OrganisationService(dbContext);
        var command = new UpdateUserOrganisationMembershipRoleCommandDto()
        {
            UserRole = UserRole.Champion,
        };
        UserOrganisationMembershipDto? result = await service.UpdateUserOrganisationMembershipRole(
            999_999,
            userOrgMembership.UserId,
            command,
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserOrganisationMembershipRole_UserDoesNotExist_ShouldReturnNull()
    {
        await using AppDbContext dbContext = CreateDbContext();
        var userOrgMembership = await SetupUserOrgMembership(dbContext);
        var service = new OrganisationService(dbContext);
        var command = new UpdateUserOrganisationMembershipRoleCommandDto()
        {
            UserRole = UserRole.Champion,
        };
        UserOrganisationMembershipDto? result = await service.UpdateUserOrganisationMembershipRole(
            userOrgMembership.OrganisationId,
            999_999,
            command,
            CancellationToken.None
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserOrganisationMembershipRoleToSuperUser_WhenUserAlreadyHasRole_ShouldThrowBadRequestException()
    {
        var existingUserRole = UserRole.Champion;
        await using AppDbContext dbContext = CreateDbContext();
        var userOrgMembership = await SetupUserOrgMembership(
            dbContext,
            x =>
            {
                x.UserRole = existingUserRole;
            }
        );
        var service = new OrganisationService(dbContext);
        var command = new UpdateUserOrganisationMembershipRoleCommandDto()
        {
            UserRole = existingUserRole,
        };
        await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            await service.UpdateUserOrganisationMembershipRole(
                userOrgMembership.OrganisationId,
                userOrgMembership.UserId,
                command,
                CancellationToken.None
            );
        });
    }

    private static async Task<UserOrgMembership> SetupUserOrgMembership(
        AppDbContext dbContext,
        Action<UserOrgMembership>? modifier = null
    )
    {
        var userOrgMembership = new UserOrgMembership()
        {
            Id = 123,
            UserId = 234,
            OrganisationId = 345,
        };
        if (modifier is not null)
        {
            modifier(userOrgMembership);
        }
        dbContext.UserOrgMemberships.Add(userOrgMembership);
        await dbContext.SaveChangesAsync();
        return userOrgMembership;
    }

    private static Organisation CreateOrganisation() =>
        new()
        {
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private static Organisation CreateOrganisationForUpdate(
        DateTime createdAt,
        DateTime lastActive
    ) =>
        new()
        {
            Id = 1,
            OrganisationName = "Old Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            HeadOfficeAddress = "Old line 1\nOld town\nOLD 1AA",
            HeadOfficeEmail = "old@example.com",
            HeadOfficeTelephone = "020 0000 0000",
            Status = UserOrgStatus.Approved,
            LastActive = lastActive,
            CreatedAt = createdAt,
        };

    private static UpdateOrganisationDetailsDto CreateUpdateDto() =>
        new()
        {
            OrganisationName = "New Pharma Ltd",
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "new@example.com",
            HeadOfficeTelephone = "020 1111 1111",
        };

    private static void AssertUpdatedDetails(
        OrganisationDetailsDto? result,
        DateTime createdAt,
        DateTime lastActive
    )
    {
        Assert.NotNull(result);
        Assert.Equal("New Pharma Ltd", result.OrganisationName);
        Assert.Equal("10 Downing Street\nLondon\nSW1A 2AA", result.HeadOfficeAddress);
        Assert.Equal("new@example.com", result.HeadOfficeEmail);
        Assert.Equal("020 1111 1111", result.HeadOfficeTelephone);
        Assert.Equal(OrganisationType.PharmaCompany, result.OrganisationType);
        Assert.Equal(PharmaceuticalEntity.Medicines, result.AllowedPharmaceuticalEntity);
        Assert.Equal(UserOrgStatus.Approved, result.Status);
        Assert.Equal(lastActive, result.LastActive);
        Assert.Equal(createdAt, result.CreatedAt);
    }

    private static void AssertUpdatedEntity(
        Organisation saved,
        DateTime createdAt,
        DateTime lastActive
    )
    {
        Assert.Equal("New Pharma Ltd", saved.OrganisationName);
        Assert.Equal("10 Downing Street\nLondon\nSW1A 2AA", saved.HeadOfficeAddress);
        Assert.Equal("new@example.com", saved.HeadOfficeEmail);
        Assert.Equal("020 1111 1111", saved.HeadOfficeTelephone);
        Assert.Equal(OrganisationType.PharmaCompany, saved.OrganisationType);
        Assert.Equal(PharmaceuticalEntity.Medicines, saved.AllowedPharmaceuticalEntity);
        Assert.Equal(UserOrgStatus.Approved, saved.Status);
        Assert.Equal(lastActive, saved.LastActive);
        Assert.Equal(createdAt, saved.CreatedAt);
    }
}
