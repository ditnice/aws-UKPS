using System.Net;
using System.Net.Http.Json;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class UserEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _membershipFaker = new();

    public UserEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        _httpClient = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_OrganisationIdProvided_ReturnsOnlyThatOrganisationsUsers()
    {
        List<Organisation> organisations = _organisationFaker.Generate(2);
        List<User> users = _userFaker.Generate(3);

        Context.Organisations.AddRange(organisations);
        Context.Users.AddRange(users);

        Context.UserOrgMemberships.AddRange(
            _membershipFaker.Generate() with
            {
                UserId = users[0].Id,
                OrganisationId = organisations[0].Id,
                Status = UserOrgStatus.RequestedAccess,
            },
            _membershipFaker.Generate() with
            {
                UserId = users[1].Id,
                OrganisationId = organisations[0].Id,
                Status = UserOrgStatus.Active,
            },
            _membershipFaker.Generate() with
            {
                UserId = users[2].Id,
                OrganisationId = organisations[1].Id,
            }
        );

        await Context.SaveChangesAsync();

        var uri = new Uri($"/users?organisationId={organisations[0].Id}", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default);
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([users[0].Id, users[1].Id], dto.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_StatusQueryParametersProvided_FiltersByStatus()
    {
        var organisation = _organisationFaker.Generate();

        var requestedUser = _userFaker.Generate() with { WorkEmail = "requested@example.com" };

        var activeUser = _userFaker.Generate() with { WorkEmail = "active@example.com" };

        var inactiveUser = _userFaker.Generate() with { WorkEmail = "inactive@example.com" };

        Context.Organisations.Add(organisation);

        Context.Users.AddRange(requestedUser, activeUser, inactiveUser);

        Context.UserOrgMemberships.AddRange(
            _membershipFaker.Generate() with
            {
                UserId = requestedUser.Id,
                OrganisationId = organisation.Id,
                Status = UserOrgStatus.RequestedAccess,
            },
            _membershipFaker.Generate() with
            {
                UserId = activeUser.Id,
                OrganisationId = organisation.Id,
                Status = UserOrgStatus.Active,
            },
            _membershipFaker.Generate() with
            {
                UserId = inactiveUser.Id,
                OrganisationId = organisation.Id,
                Status = UserOrgStatus.Inactive,
            }
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
