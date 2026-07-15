using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data.Fakers;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.GetOrganisationByIdError
>;
using UpdateOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.UpdateOrganisationDetailsError
>;

namespace UKPS.Api.Tests.Services.Organisations;

[Collection(DatabaseCollection.Name)]
public abstract class OrganisationServiceTests : DatabaseTestBase
{
    internal abstract IServiceTestHarness<IOrganisationService> ServiceTestHarness { get; }
    private IOrganisationService Service => ServiceTestHarness.Service;

    internal readonly OrganisationFaker _organisationFaker = new();

    protected OrganisationServiceTests(PostgresFixture fixture)
        : base(fixture) { }

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
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        int id = (
            await Context.Organisations.SingleAsync(TestContext.Current.CancellationToken)
        ).Id;

        GetOrganisationResult result = await Service.GetOrganisationById(
            id,
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(id);
        dto.OrganisationName.ShouldBe("Gov Pharma Ltd");
        dto.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Medicines);
        dto.OrganisationType.ShouldBe(OrganisationType.PharmaCompany);
        dto.HeadOfficeAddress.ShouldBe("10 Downing Street\nLondon\nSW1A 2AA");
        dto.HeadOfficeEmail.ShouldBe("info@pharma.gov.uk");
        dto.HeadOfficeTelephone.ShouldBe("020 1234 5678");
        dto.Status.ShouldBe(UserOrgStatus.Active);
        dto.LastActive.ShouldBe(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc));
        dto.CreatedAt.ShouldBe(new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(true, UserRole.Super, true)]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(true, UserRole.Standard, true)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task GetOrganisationById_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        Organisation organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        int otherOrganisationId = 999_999;
        int usersOrganisationId = organisationIdMatches ? organisation.Id : otherOrganisationId;

        var testHarness = ServiceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = usersOrganisationId,
                UserRole = userRole,
            }
        );
        var service = testHarness.Service;

        GetOrganisationResult result = await service.GetOrganisationById(
            organisation.Id,
            TestContext.Current.CancellationToken
        );

        if (expectedAuthorised)
        {
            result.IsOk.ShouldBeTrue();
        }
        else
        {
            result.IsErr.ShouldBeTrue();
            result.Error.ShouldBeOfType<GetOrganisationByIdError.NotAllowed>();
        }
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFoundError()
    {
        Context.Organisations.Add(CreateOrganisation());
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        int seededId = (
            await Context.Organisations.SingleAsync(TestContext.Current.CancellationToken)
        ).Id;

        GetOrganisationResult result = await Service.GetOrganisationById(
            seededId + 1,
            TestContext.Current.CancellationToken
        );

        GetOrganisationByIdError.NotFound notFound = result
            .ShouldBeError()
            .ShouldBeOfType<GetOrganisationByIdError.NotFound>();
        notFound.OrganisationId.ShouldBe(seededId + 1);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_UpdatesEditableFields()
    {
        DateTime createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
        DateTime lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
        Context.Organisations.Add(CreateOrganisationForUpdate(createdAt, lastActive));
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        int id = (
            await Context.Organisations.SingleAsync(TestContext.Current.CancellationToken)
        ).Id;

        UpdateOrganisationResult result = await Service.UpdateOrganisationDetails(
            id,
            CreateUpdateDto(),
            TestContext.Current.CancellationToken
        );

        result.IsOk.ShouldBeTrue();
        AssertUpdatedDetails(result.Value, createdAt, lastActive);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        Organisation saved = await verifyContext.Organisations.SingleAsync(
            o => o.Id == id,
            TestContext.Current.CancellationToken
        );
        AssertUpdatedEntity(saved, createdAt, lastActive);
    }

    [Theory]
    [InlineData(true, UserRole.Super, true)]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(true, UserRole.Standard, false)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task UpdateOrganisationDetails_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        Organisation organisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        int otherOrganisationId = 999_999;
        int usersOrganisationId = organisationIdMatches ? organisation.Id : otherOrganisationId;

        var testHarness = ServiceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = usersOrganisationId,
                UserRole = userRole,
            }
        );
        var service = testHarness.Service;

        var updateCommand = new UpdateOrganisationDetailsDtoFaker().Generate();
        UpdateOrganisationResult result = await service.UpdateOrganisationDetails(
            organisation.Id,
            updateCommand,
            TestContext.Current.CancellationToken
        );

        if (expectedAuthorised)
        {
            result.IsOk.ShouldBeTrue();
        }
        else
        {
            result.IsErr.ShouldBeTrue();
            result.Error.ShouldBeOfType<UpdateOrganisationDetailsError.NotAllowed>();
        }
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFoundError()
    {
        UpdateOrganisationResult result = await Service.UpdateOrganisationDetails(
            99,
            new UpdateOrganisationDetailsDto
            {
                OrganisationName = "New Pharma Ltd",
                HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
                HeadOfficeEmail = "new@example.com",
                HeadOfficeTelephone = "020 1111 1111",
            },
            TestContext.Current.CancellationToken
        );

        result.IsErr.ShouldBeTrue();
        UpdateOrganisationDetailsError.NotFound notFound =
            result.Error.ShouldBeOfType<UpdateOrganisationDetailsError.NotFound>();
        notFound.OrganisationId.ShouldBe(99);
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
        result.ShouldNotBeNull();
        result.OrganisationName.ShouldBe("New Pharma Ltd");
        result.HeadOfficeAddress.ShouldBe("10 Downing Street\nLondon\nSW1A 2AA");
        result.HeadOfficeEmail.ShouldBe("new@example.com");
        result.HeadOfficeTelephone.ShouldBe("020 1111 1111");
        result.OrganisationType.ShouldBe(OrganisationType.PharmaCompany);
        result.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Medicines);
        result.Status.ShouldBe(UserOrgStatus.Active);
        result.LastActive.ShouldBe(lastActive);
        result.CreatedAt.ShouldBe(createdAt);
    }

    private static void AssertUpdatedEntity(
        Organisation saved,
        DateTime createdAt,
        DateTime lastActive
    )
    {
        saved.OrganisationName.ShouldBe("New Pharma Ltd");
        saved.HeadOfficeAddress.ShouldBe("10 Downing Street\nLondon\nSW1A 2AA");
        saved.HeadOfficeEmail.ShouldBe("new@example.com");
        saved.HeadOfficeTelephone.ShouldBe("020 1111 1111");
        saved.OrganisationType.ShouldBe(OrganisationType.PharmaCompany);
        saved.AllowedPharmaceuticalEntity.ShouldBe(PharmaceuticalEntity.Medicines);
        saved.Status.ShouldBe(UserOrgStatus.Active);
        saved.LastActive.ShouldBe(lastActive);
        saved.CreatedAt.ShouldBe(createdAt);
    }

    protected internal sealed class UpdateOrganisationDetailsDtoFaker
        : Faker<UpdateOrganisationDetailsDto>
    {
        public UpdateOrganisationDetailsDtoFaker()
        {
            RuleFor(o => o.OrganisationName, f => f.Company.CompanyName());
            RuleFor(o => o.HeadOfficeAddress, f => f.Address.FullAddress());
            RuleFor(o => o.HeadOfficeEmail, f => f.Internet.Email());
            RuleFor(o => o.HeadOfficeTelephone, f => f.Phone.PhoneNumber());
        }
    }
}
