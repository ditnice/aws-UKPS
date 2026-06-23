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
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetOrganisationByIdAsync_ReturnsDto_WhenOrganisationExists()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(new Organisation
        {
            Id = 1,
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            HeadOfficeAddress = "1 High Street, London, EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
            HeadOfficeTelephone = "020 1234 5678",
            Status = UserOrgStatus.Approved,
            LastActive = new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc),
        });
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Acme Pharma Ltd", result.OrganisationName);
        Assert.Equal(PharmaceuticalEntity.Medicines, result.AllowedPharmaceuticalEntity);
        Assert.Equal(OrganisationType.PharmaCompany, result.OrganisationType);
        Assert.Equal("1 High Street, London, EC1A 1AA", result.HeadOfficeAddress);
        Assert.Equal("info@acme.com", result.HeadOfficeEmail);
        Assert.Equal("020 1234 5678", result.HeadOfficeTelephone);
        Assert.Equal(UserOrgStatus.Approved, result.Status);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), result.LastActive);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), result.CreatedAt);
    }

    [Fact]
    public async Task GetOrganisationById_ReturnsNull_WhenOrganisationDoesNotExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(new Organisation
        {
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany
        });
        await dbContext.SaveChangesAsync();
        int seededId = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(seededId + 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganisationById_ReturnsNullFields_WhenOptionalFieldsNotSet()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(new Organisation
        {
            OrganisationName = "Minimal Org",
            OrganisationType = OrganisationType.Internal
        });
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetOrganisationById(id);

        Assert.NotNull(result);
        Assert.Null(result.HeadOfficeAddress);
        Assert.Null(result.HeadOfficeEmail);
        Assert.Null(result.HeadOfficeTelephone);
    }
}
