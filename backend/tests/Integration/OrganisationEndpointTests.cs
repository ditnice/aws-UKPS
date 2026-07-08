using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;
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
        await Context.SaveChangesAsync();

        var uri = new Uri("/organisations/1", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        OrganisationDetailsDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationDetailsDto>(
                TestJsonOptions.Default
            );
        Assert.NotNull(dto);
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
        Assert.Equal(expectedDto, dto);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFound()
    {
        var uri = new Uri("/organisations/999999", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_ValidBody_ReturnsOkAndPersistsChanges()
    {
        Organisation organisation = new OrganisationFaker().Generate() with { Id = 1 };
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

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
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        OrganisationDetailsDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationDetailsDto>(
                TestJsonOptions.Default
            );
        Assert.NotNull(dto);
        Assert.Equal("New Pharma Ltd", dto.OrganisationName);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        Organisation saved = await verifyContext.Organisations.SingleAsync(o => o.Id == 1);
        Assert.Equal("New Pharma Ltd", saved.OrganisationName);
        Assert.Equal("1 New Street\nLondon\nEC1A 1AA", saved.HeadOfficeAddress);
        Assert.Equal("new@example.com", saved.HeadOfficeEmail);
        Assert.Equal("020 9999 9999", saved.HeadOfficeTelephone);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFound()
    {
        UpdateOrganisationDetailsDto updateDto = CreateValidUpdateDto();
        var uri = new Uri("/organisations/999999", UriKind.Relative);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            uri,
            updateDto,
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_InvalidEmail_ReturnsBadRequest()
    {
        Organisation organisation = new OrganisationFaker().Generate() with { Id = 1 };
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

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
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_WhitespaceOnlyAddress_ReturnsBadRequest()
    {
        Organisation organisation = new OrganisationFaker().Generate() with { Id = 1 };
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

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
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateMembership_ValidRequest_ReturnsOkAndPersistsInactiveStatus()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/{membership.OrganisationId}/memberships/{membership.Id}/deactivate",
            UriKind.Relative
        );

        HttpResponseMessage response = await _httpClient.PatchAsync(uri, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        OrganisationMembershipDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationMembershipDto>(
                TestJsonOptions.Default
            );
        Assert.NotNull(dto);
        Assert.Equal(UserOrgStatus.Inactive, dto.Status);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == membership.Id
        );
        Assert.Equal(UserOrgStatus.Inactive, saved.Status);
    }

    [Fact]
    public async Task DeactivateMembership_OrganisationIdDoesNotMatch_ReturnsNotFound()
    {
        UserOrgMembership membership = await SeedMembership();
        var uri = new Uri(
            $"/organisations/999999/memberships/{membership.Id}/deactivate",
            UriKind.Relative
        );

        HttpResponseMessage response = await _httpClient.PatchAsync(uri, null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        OrganisationMembershipDto? dto =
            await response.Content.ReadFromJsonAsync<OrganisationMembershipDto>(
                TestJsonOptions.Default
            );
        Assert.NotNull(dto);
        Assert.Equal(UserRole.Champion, dto.UserRole);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == membership.Id
        );
        Assert.Equal(UserRole.Champion, saved.UserRole);
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
            TestJsonOptions.Default
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

        HttpResponseMessage response = await _httpClient.PatchAsync(uri, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        User user = new UserFaker().Generate() with
        {
            Id = 1,
            WorkEmail = "member@example.com",
        };
        Context.Users.Add(user);
        Organisation organisation = new OrganisationFaker().Generate() with { Id = 1 };
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

        UserOrgMembership membership = new UserOrgMembershipFaker().Generate() with
        {
            Id = 1,
            UserId = 1,
            OrganisationId = 1,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
        };
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync();
        return membership;
    }
}
