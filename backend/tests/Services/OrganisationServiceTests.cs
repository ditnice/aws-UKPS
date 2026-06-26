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
                OrganisationName = "Acme Pharma Ltd",
                OrganisationType = OrganisationType.PharmaCompany,
                AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
                HeadOfficeAddressLine1 = "1 High Street",
                HeadOfficeAddressLine2 = "Floor 2",
                HeadOfficeTown = "London",
                HeadOfficeCounty = "Greater London",
                HeadOfficePostcode = "EC1A 1AA",
                HeadOfficeEmail = "info@acme.com",
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
        Assert.Equal("Acme Pharma Ltd", result.OrganisationName);
        Assert.Equal(PharmaceuticalEntity.Medicines, result.AllowedPharmaceuticalEntity);
        Assert.Equal(OrganisationType.PharmaCompany, result.OrganisationType);
        Assert.Equal("1 High Street", result.HeadOfficeAddressLine1);
        Assert.Equal("Floor 2", result.HeadOfficeAddressLine2);
        Assert.Equal("London", result.HeadOfficeTown);
        Assert.Equal("Greater London", result.HeadOfficeCounty);
        Assert.Equal("EC1A 1AA", result.HeadOfficePostcode);
        Assert.Equal("info@acme.com", result.HeadOfficeEmail);
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
    public async Task GetOrganisationById_OptionalFieldsNotSet_ReturnsNullFields()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(CreateOrganisation());
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(id);

        Assert.NotNull(result);
        Assert.Null(result.HeadOfficeAddressLine2);
        Assert.Null(result.HeadOfficeCounty);
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
    public async Task UpdateOrganisationDetails_OptionalAddressFieldsAreNull_AllowsNullFields()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(
            new Organisation
            {
                OrganisationName = "Old Pharma Ltd",
                OrganisationType = OrganisationType.PharmaCompany,
                HeadOfficeAddressLine1 = "Old line 1",
                HeadOfficeAddressLine2 = "Old line 2",
                HeadOfficeTown = "Old town",
                HeadOfficeCounty = "Old county",
                HeadOfficePostcode = "OLD 1AA",
                HeadOfficeEmail = "old@example.com",
                HeadOfficeTelephone = "020 0000 0000",
            }
        );
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.UpdateOrganisationDetails(
            id,
            new UpdateOrganisationDetailsDto
            {
                OrganisationName = "New Pharma Ltd",
                HeadOfficeAddressLine1 = "New line 1",
                HeadOfficeTown = "New town",
                HeadOfficePostcode = "NEW 1AA",
                HeadOfficeEmail = "new@example.com",
                HeadOfficeTelephone = "020 1111 1111",
            }
        );

        Assert.NotNull(result);
        Assert.Null(result.HeadOfficeAddressLine2);
        Assert.Null(result.HeadOfficeCounty);
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
                HeadOfficeAddressLine1 = "New line 1",
                HeadOfficeTown = "New town",
                HeadOfficePostcode = "NEW 1AA",
                HeadOfficeEmail = "new@example.com",
                HeadOfficeTelephone = "020 1111 1111",
            }
        );

        Assert.Null(result);
    }

    private static Organisation CreateOrganisation() =>
        new()
        {
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddressLine1 = "1 High Street",
            HeadOfficeTown = "London",
            HeadOfficePostcode = "EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
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
            HeadOfficeAddressLine1 = "Old line 1",
            HeadOfficeAddressLine2 = "Old line 2",
            HeadOfficeTown = "Old town",
            HeadOfficeCounty = "Old county",
            HeadOfficePostcode = "OLD 1AA",
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
            HeadOfficeAddressLine1 = "New line 1",
            HeadOfficeAddressLine2 = "New line 2",
            HeadOfficeTown = "New town",
            HeadOfficeCounty = "New county",
            HeadOfficePostcode = "NEW 1AA",
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
        Assert.Equal("New line 1", result.HeadOfficeAddressLine1);
        Assert.Equal("New line 2", result.HeadOfficeAddressLine2);
        Assert.Equal("New town", result.HeadOfficeTown);
        Assert.Equal("New county", result.HeadOfficeCounty);
        Assert.Equal("NEW 1AA", result.HeadOfficePostcode);
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
        Assert.Equal("New line 1", saved.HeadOfficeAddressLine1);
        Assert.Equal("New line 2", saved.HeadOfficeAddressLine2);
        Assert.Equal("New town", saved.HeadOfficeTown);
        Assert.Equal("New county", saved.HeadOfficeCounty);
        Assert.Equal("NEW 1AA", saved.HeadOfficePostcode);
        Assert.Equal("new@example.com", saved.HeadOfficeEmail);
        Assert.Equal("020 1111 1111", saved.HeadOfficeTelephone);
        Assert.Equal(OrganisationType.PharmaCompany, saved.OrganisationType);
        Assert.Equal(PharmaceuticalEntity.Medicines, saved.AllowedPharmaceuticalEntity);
        Assert.Equal(UserOrgStatus.Approved, saved.Status);
        Assert.Equal(lastActive, saved.LastActive);
        Assert.Equal(createdAt, saved.CreatedAt);
    }
}
