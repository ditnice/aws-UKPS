using System.Net;
using System.Net.Http.Json;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class UserEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;

    public UserEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        _httpClient = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_OrganisationIdProvided_ReturnsOnlyThatOrganisationsUsers()
    {
        Context.Organisations.AddRange(
            EntityFactory.CreateOrganisation(id: 1),
            EntityFactory.CreateOrganisation(id: 2)
        );
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 1, workEmail: "requested@example.com"),
            EntityFactory.CreateUser(id: 2, workEmail: "active@example.com"),
            EntityFactory.CreateUser(id: 3, workEmail: "other-org@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 1,
                organisationId: 1,
                status: UserOrgStatus.RequestedAccess
            ),
            EntityFactory.CreateMembership(
                id: 2,
                userId: 2,
                organisationId: 1,
                status: UserOrgStatus.Active
            ),
            EntityFactory.CreateMembership(id: 3, userId: 3, organisationId: 2)
        );
        await Context.SaveChangesAsync();

        var uri = new Uri("/users?organisationId=1", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default);
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([1, 2], dto.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_StatusQueryParametersProvided_FiltersByStatus()
    {
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 1));
        Context.Users.AddRange(
            EntityFactory.CreateUser(id: 1, workEmail: "requested@example.com"),
            EntityFactory.CreateUser(id: 2, workEmail: "active@example.com"),
            EntityFactory.CreateUser(id: 3, workEmail: "inactive@example.com")
        );
        Context.UserOrgMemberships.AddRange(
            EntityFactory.CreateMembership(
                id: 1,
                userId: 1,
                organisationId: 1,
                status: UserOrgStatus.RequestedAccess
            ),
            EntityFactory.CreateMembership(
                id: 2,
                userId: 2,
                organisationId: 1,
                status: UserOrgStatus.Active
            ),
            EntityFactory.CreateMembership(
                id: 3,
                userId: 3,
                organisationId: 1,
                status: UserOrgStatus.Inactive
            )
        );
        await Context.SaveChangesAsync();

        var uri = new Uri(
            "/users?organisationId=1&status=Active&status=Inactive",
            UriKind.Relative
        );
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default);
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([2, 3], dto.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_StatusQueryParameterIsInvalid_ReturnsBadRequest()
    {
        var uri = new Uri("/users?status=NotAStatus", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_PageIsZero_ReturnsBadRequest()
    {
        var uri = new Uri("/users?page=0", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_PageSizeExceedsMaximum_ReturnsBadRequest()
    {
        var uri = new Uri("/users?pageSize=101", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_OrganisationDoesNotExist_ReturnsBadRequest()
    {
        var uri = new Uri("/users?organisationId=999999", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
