using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;

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
                Status = UserOrgStatus.Active,
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
        Assert.Equal(UserOrgStatus.Active, result.Status);
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

    [Fact]
    public async Task DeactivateUser_OrganisationDoesNotExist_ReturnsOrganisationNotFound()
    {
        await using AppDbContext dbContext = CreateDbContext();
        OrganisationService service = new(dbContext);

        DeactivateOrganisationUserResult result = await service.DeactivateUser(99, 10);

        Assert.Equal(DeactivateOrganisationUserStatus.OrganisationNotFound, result.Status);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task DeactivateUser_UserDoesNotExistInOrganisation_ReturnsUserNotFound()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        await dbContext.SaveChangesAsync();
        OrganisationService service = new(dbContext);

        DeactivateOrganisationUserResult result = await service.DeactivateUser(1, 99);

        Assert.Equal(DeactivateOrganisationUserStatus.UserNotFound, result.Status);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task DeactivateUser_UserExistsInDifferentOrganisation_ReturnsUserNotFound()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.AddRange(CreateOrganisation(id: 1), CreateOrganisation(id: 2));
        dbContext.Users.Add(CreateUser(id: 10, workEmail: "user@example.com"));
        dbContext.UserOrgMemberships.Add(CreateMembership(id: 100, userId: 10, organisationId: 2));
        await dbContext.SaveChangesAsync();
        OrganisationService service = new(dbContext);

        DeactivateOrganisationUserResult result = await service.DeactivateUser(1, 10);

        Assert.Equal(DeactivateOrganisationUserStatus.UserNotFound, result.Status);
        UserOrgMembership saved = await dbContext.UserOrgMemberships.SingleAsync(m => m.Id == 100);
        Assert.Equal(UserOrgStatus.Active, saved.Status);
    }

    [Fact]
    public async Task DeactivateUser_UserAlreadyInactive_ReturnsAlreadyInactive()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation(id: 1));
        dbContext.Users.Add(CreateUser(id: 10, workEmail: "user@example.com"));
        dbContext.UserOrgMemberships.Add(
            CreateMembership(id: 100, userId: 10, organisationId: 1, status: UserOrgStatus.Inactive)
        );
        await dbContext.SaveChangesAsync();
        OrganisationService service = new(dbContext);

        DeactivateOrganisationUserResult result = await service.DeactivateUser(1, 10);

        Assert.Equal(DeactivateOrganisationUserStatus.AlreadyInactive, result.Status);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task DeactivateUser_UserIsActive_UpdatesMembershipStatusToInactive()
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
        OrganisationService service = new(dbContext);

        DeactivateOrganisationUserResult result = await service.DeactivateUser(1, 10);

        Assert.Equal(DeactivateOrganisationUserStatus.Success, result.Status);
        Assert.NotNull(result.User);
        Assert.Equal(10, result.User.UserId);
        Assert.Equal("user@example.com", result.User.EmailAddress);
        Assert.Equal(UserRole.Champion, result.User.Role);
        Assert.Equal(UserOrgStatus.Inactive, result.User.Status);
        Assert.Null(result.User.LastActive);

        UserOrgMembership saved = await dbContext.UserOrgMemberships.SingleAsync(m => m.Id == 100);
        Assert.Equal(UserOrgStatus.Inactive, saved.Status);
    }

    private static Organisation CreateOrganisation() => CreateOrganisation(id: 1);

    private static Organisation CreateOrganisation(int id) =>
        new()
        {
            Id = id,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
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
            Status = UserOrgStatus.Active,
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
        Assert.Equal(UserOrgStatus.Active, result.Status);
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
        Assert.Equal(UserOrgStatus.Active, saved.Status);
        Assert.Equal(lastActive, saved.LastActive);
        Assert.Equal(createdAt, saved.CreatedAt);
    }
}
