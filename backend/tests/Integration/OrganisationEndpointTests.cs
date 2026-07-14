using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class OrganisationEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;

    public OrganisationEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        _httpClient = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationExists_ReturnsOkWithDto()
    {
        DateTime createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
        DateTime lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
        var organisation = new Organisation
        {
            Id = 1,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
            Status = UserOrgStatus.Active,
            LastActive = lastActive,
            CreatedAt = createdAt,
        };
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uri = new Uri("/organisations/1", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        OrganisationDetailsDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationDetailsDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );
        dto.ShouldNotBeNull();
        var expectedDto = new OrganisationDetailsDto
        {
            Id = organisation.Id,
            OrganisationName = organisation.OrganisationName,
            OrganisationType = organisation.OrganisationType,
            AllowedPharmaceuticalEntity = organisation.AllowedPharmaceuticalEntity,
            HeadOfficeAddress = organisation.HeadOfficeAddress,
            HeadOfficeEmail = organisation.HeadOfficeEmail,
            HeadOfficeTelephone = organisation.HeadOfficeTelephone,
            Status = organisation.Status,
            LastActive = organisation.LastActive,
            CreatedAt = organisation.CreatedAt,
        };
        dto.ShouldBe(expectedDto);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFound()
    {
        var uri = new Uri("/organisations/999999", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_ValidBody_ReturnsOkAndPersistsChanges()
    {
        Organisation organisation = new OrganisationFaker().Generate().Update(x => x.Id = 1);
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        UpdateOrganisationDetailsDto updateDto = new()
        {
            OrganisationName = "New Pharma Ltd",
            HeadOfficeAddress = "1 New Street\nLondon\nEC1A 1AA",
            HeadOfficeEmail = "new@example.com",
            HeadOfficeTelephone = "020 9999 9999",
        };
        var uri = new Uri("/organisations/1", UriKind.Relative);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            uri,
            updateDto,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        OrganisationDetailsDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationDetailsDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );
        dto.ShouldNotBeNull();
        dto.OrganisationName.ShouldBe("New Pharma Ltd");

        await using AppDbContext verifyContext = Fixture.CreateContext();
        Organisation saved = await verifyContext.Organisations.SingleAsync(
            o => o.Id == 1,
            TestContext.Current.CancellationToken
        );
        saved.OrganisationName.ShouldBe("New Pharma Ltd");
        saved.HeadOfficeAddress.ShouldBe("1 New Street\nLondon\nEC1A 1AA");
        saved.HeadOfficeEmail.ShouldBe("new@example.com");
        saved.HeadOfficeTelephone.ShouldBe("020 9999 9999");
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFound()
    {
        UpdateOrganisationDetailsDto updateDto = CreateValidUpdateDto();
        var uri = new Uri("/organisations/999999", UriKind.Relative);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            uri,
            updateDto,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_InvalidEmail_ReturnsBadRequest()
    {
        Organisation organisation = new OrganisationFaker().Generate().Update(x => x.Id = 1);
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        UpdateOrganisationDetailsDto updateDto = new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "1 High Street",
            HeadOfficeEmail = "not-an-email",
            HeadOfficeTelephone = "020 1234 5678",
        };
        var uri = new Uri("/organisations/1", UriKind.Relative);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            uri,
            updateDto,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_WhitespaceOnlyAddress_ReturnsBadRequest()
    {
        Organisation organisation = new OrganisationFaker().Generate().Update(x => x.Id = 1);
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        UpdateOrganisationDetailsDto updateDto = new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "   ",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };
        var uri = new Uri("/organisations/1", UriKind.Relative);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            uri,
            updateDto,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivateMembership_ValidRequest_ReturnsOkAndPersistsInactiveStatus()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/{membership.OrganisationId}/memberships/{membership.Id}/deactivate",
            UriKind.Relative
        );

        HttpResponseMessage response = await _httpClient.PatchAsync(
            uri,
            null,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        OrganisationMembershipDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationMembershipDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );
        dto.ShouldNotBeNull();
        dto.Status.ShouldBe(UserOrgStatus.Inactive);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == membership.Id,
            TestContext.Current.CancellationToken
        );
        saved.Status.ShouldBe(UserOrgStatus.Inactive);
    }

    [Fact]
    public async Task DeactivateMembership_OrganisationIdDoesNotMatch_ReturnsNotFound()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/999999/memberships/{membership.Id}/deactivate",
            UriKind.Relative
        );

        HttpResponseMessage response = await _httpClient.PatchAsync(
            uri,
            null,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserRole_ValidRequest_ReturnsOkAndPersistsRole()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/{membership.OrganisationId}/memberships/{membership.Id}/update-role",
            UriKind.Relative
        );
        UpdateOrgMembershipUserRoleCommandDto command = new() { UserRole = UserRole.Champion };

        HttpResponseMessage response = await _httpClient.PatchAsJsonAsync(
            uri,
            command,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        OrganisationMembershipDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationMembershipDto>(
                TestJsonOptions.Default,
                TestContext.Current.CancellationToken
            );
        dto.ShouldNotBeNull();
        dto.UserRole.ShouldBe(UserRole.Champion);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == membership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(UserRole.Champion);
    }

    [Fact]
    public async Task UpdateUserRole_MembershipDoesNotExist_ReturnsNotFound()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/{membership.OrganisationId}/memberships/999999/update-role",
            UriKind.Relative
        );
        UpdateOrgMembershipUserRoleCommandDto command = new() { UserRole = UserRole.Champion };

        HttpResponseMessage response = await _httpClient.PatchAsJsonAsync(
            uri,
            command,
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserRole_InvalidEnumValue_ReturnsBadRequest()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/{membership.OrganisationId}/memberships/{membership.Id}/update-role",
            UriKind.Relative
        );
        using StringContent content = new(
            """{"userRole":"NotARole"}""",
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await _httpClient.PatchAsync(
            uri,
            content,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static UpdateOrganisationDetailsDto CreateValidUpdateDto() =>
        new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "1 High Street",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private async Task<UserOrgMembership> SeedMembership()
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        User user = new UserFaker()
            .Generate()
            .Update(x =>
            {
                x.Id = 1;
                x.WorkEmail = "member@example.com";
            });
        Context.Users.Add(user);
        Organisation organisation = new OrganisationFaker().Generate().Update(x => x.Id = 1);
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

        UserOrgMembership membership = new UserOrgMembershipFaker()
            .Generate()
            .Update(x =>
            {
                x.Id = 1;
                x.UserId = 1;
                x.OrganisationId = 1;
                x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both;
            });
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync();
        return membership;
    }
}
