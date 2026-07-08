using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UKPS.Api.Common;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class OrganisationServiceTests : DatabaseTestBase
{
    private readonly OrganisationService _service;

    public OrganisationServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _service = new OrganisationService(
            Context,
            Substitute.For<IOrganisationMembershipService>()
        );
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationExists_ReturnsDto()
    {
        Context.Organisations.Add(
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
        await Context.SaveChangesAsync();
        int id = (await Context.Organisations.SingleAsync()).Id;

        Result<OrganisationDetailsDto, GetOrganisationByIdError> result =
            await _service.GetOrganisationById(id);

        Assert.True(result.IsOk);
        OrganisationDetailsDto? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Gov Pharma Ltd", dto.OrganisationName);
        Assert.Equal(PharmaceuticalEntity.Medicines, dto.AllowedPharmaceuticalEntity);
        Assert.Equal(OrganisationType.PharmaCompany, dto.OrganisationType);
        Assert.Equal("10 Downing Street\nLondon\nSW1A 2AA", dto.HeadOfficeAddress);
        Assert.Equal("info@pharma.gov.uk", dto.HeadOfficeEmail);
        Assert.Equal("020 1234 5678", dto.HeadOfficeTelephone);
        Assert.Equal(UserOrgStatus.Active, dto.Status);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), dto.LastActive);
        Assert.Equal(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc), dto.CreatedAt);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFoundError()
    {
        Context.Organisations.Add(CreateOrganisation());
        await Context.SaveChangesAsync();
        int seededId = (await Context.Organisations.SingleAsync()).Id;

        Result<OrganisationDetailsDto, GetOrganisationByIdError> result =
            await _service.GetOrganisationById(seededId + 1);

        Assert.True(result.IsErr);
        GetOrganisationByIdError.NotFound notFound =
            Assert.IsType<GetOrganisationByIdError.NotFound>(result.Error);
        Assert.Equal(seededId + 1, notFound.OrganisationId);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_UpdatesEditableFields()
    {
        DateTime createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
        DateTime lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
        Context.Organisations.Add(CreateOrganisationForUpdate(createdAt, lastActive));
        await Context.SaveChangesAsync();
        int id = (await Context.Organisations.SingleAsync()).Id;

        Result<OrganisationDetailsDto, UpdateOrganisationDetailsError> result =
            await _service.UpdateOrganisationDetails(id, CreateUpdateDto());

        Assert.True(result.IsOk);
        AssertUpdatedDetails(result.Value, createdAt, lastActive);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        Organisation saved = await verifyContext.Organisations.SingleAsync(o => o.Id == id);
        AssertUpdatedEntity(saved, createdAt, lastActive);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFoundError()
    {
        Result<OrganisationDetailsDto, UpdateOrganisationDetailsError> result =
            await _service.UpdateOrganisationDetails(
                99,
                new UpdateOrganisationDetailsDto
                {
                    OrganisationName = "New Pharma Ltd",
                    HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
                    HeadOfficeEmail = "new@example.com",
                    HeadOfficeTelephone = "020 1111 1111",
                }
            );

        Assert.True(result.IsErr);
        UpdateOrganisationDetailsError.NotFound notFound =
            Assert.IsType<UpdateOrganisationDetailsError.NotFound>(result.Error);
        Assert.Equal(99, notFound.OrganisationId);
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
