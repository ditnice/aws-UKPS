using System.Net;
using System.Net.Http.Json;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Data.Fakers;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data;

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
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = users[0].Id;
                    x.OrganisationId = organisations[0].Id;
                    x.Status = UserOrgStatus.RequestedAccess;
                }),
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = users[1].Id;
                    x.OrganisationId = organisations[0].Id;
                    x.Status = UserOrgStatus.Active;
                }),
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = users[2].Id;
                    x.OrganisationId = organisations[1].Id;
                })
        );

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uri = new Uri($"/users?organisationId={organisations[0].Id}", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default, TestContext.Current.CancellationToken);
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([users[0].Id, users[1].Id]);
    }

    [Fact]
    public async Task GetUsers_StatusQueryParametersProvided_FiltersByStatus()
    {
        var organisation = _organisationFaker.Generate();
        var requestedUser = _userFaker.Generate();
        var activeUser = _userFaker.Generate();
        var inactiveUser = _userFaker.Generate();

        Context.Organisations.Add(organisation);

        Context.Users.AddRange(requestedUser, activeUser, inactiveUser);

        Context.UserOrgMemberships.AddRange(
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = requestedUser.Id;
                    x.OrganisationId = organisation.Id;
                    x.Status = UserOrgStatus.RequestedAccess;
                }),
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = activeUser.Id;
                    x.OrganisationId = organisation.Id;
                    x.Status = UserOrgStatus.Active;
                }),
            _membershipFaker
                .Generate()
                .Update(x =>
                {
                    x.UserId = inactiveUser.Id;
                    x.OrganisationId = organisation.Id;
                    x.Status = UserOrgStatus.Inactive;
                })
        );

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uri = new Uri(
            "/users?organisationId=1&status=Active&status=Inactive",
            UriKind.Relative
        );
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default, TestContext.Current.CancellationToken);
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([2, 3]);
    }

    [Fact]
    public async Task GetUsers_StatusQueryParameterIsInvalid_ReturnsBadRequest()
    {
        var uri = new Uri("/users?status=NotAStatus", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_PageIsZero_ReturnsBadRequest()
    {
        var uri = new Uri("/users?page=0", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_PageSizeExceedsMaximum_ReturnsBadRequest()
    {
        var uri = new Uri("/users?pageSize=101", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_OrganisationDoesNotExist_ReturnsBadRequest()
    {
        var uri = new Uri("/users?organisationId=999999", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
