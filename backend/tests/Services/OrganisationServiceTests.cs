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
    public async Task GetByIdAsync_ReturnsDto_WhenOrganisationExists()
    {
        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Organisations.Add(new Organisation
        {
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            HeadOfficeAddress = "1 High Street, London, EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
            HeadOfficeTelephone = "020 1234 5678"
        });
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("PharmaCompany", result.OrganisationType.ToString());
        Assert.Equal("Acme Pharma Ltd", result.OrganisationName);
        Assert.Equal("1 High Street, London, EC1A 1AA", result.HeadOfficeAddress);
        Assert.Equal("info@acme.com", result.HeadOfficeEmail);
        Assert.Equal("020 1234 5678", result.HeadOfficeTelephone);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOrganisationDoesNotExist()
    {
        await using AppDbContext dbContext = CreateDbContext();
        OrganisationService service = new(dbContext);

        OrganisationDetailsDto? result = await service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDifferentOrganisationExists()
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
        OrganisationDetailsDto? result = await service.GetByIdAsync(seededId + 1);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("PharmaCompany")]
    [InlineData("HorizonScanning")]
    [InlineData("Strategic")]
    [InlineData("Internal")]
    public async Task GetByIdAsync_MapsOrganisationTypeToString(string organisationType)
    {
        await using AppDbContext dbContext = CreateDbContext();
        OrganisationType type = Enum.Parse<OrganisationType>(organisationType);
        dbContext.Organisations.Add(new Organisation
        {
            OrganisationName = "Test Org",
            OrganisationType = type
        });
        await dbContext.SaveChangesAsync();
        int id = (await dbContext.Organisations.SingleAsync()).Id;

        OrganisationService service = new(dbContext);
        OrganisationDetailsDto? result = await service.GetByIdAsync(id);

        Assert.Equal(organisationType, result!.OrganisationType);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullFields_WhenOptionalFieldsNotSet()
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
        OrganisationDetailsDto? result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Null(result.HeadOfficeAddress);
        Assert.Null(result.HeadOfficeEmail);
        Assert.Null(result.HeadOfficeTelephone);
    }
}
