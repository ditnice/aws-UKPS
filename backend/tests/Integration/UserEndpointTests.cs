using System.Net;
using System.Net.Http.Json;
using Bogus;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class UserEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserOrgMembershipFaker _membershipFaker = new();
    private readonly Randomizer _rng = new Randomizer(5);

    private IReadOnlyCollection<User> _seededUsers = null!;
    private IEnumerable<Organisation> SeededOrganisations =>
        _seededUsers
            .SelectMany(x => x.UserOrgMemberships!)
            .Select(x => x.Organisation!)
            .DistinctBy(x => x.Id);

    public UserEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        _httpClient = fixture.Factory.CreateClient();
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var organisations = _organisationFaker.Generate(3);
        var userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            f =>
            {
                return f.PickRandom(organisations, f.Random.Int(1, 3))
                    .Select(o => _membershipFaker.RuleFor(x => x.Organisation, _ => o).Generate())
                    .ToArray();
            }
        );
        var users = userFaker.Generate(50);
        _seededUsers = (await AddEntities(users, TestContext.Current.CancellationToken)).ToList();
    }

    [Fact]
    public async Task GetUsers_OrganisationIdProvided_ReturnsOnlyThatOrganisationsUsers()
    {
        var sampleOrganisation = _rng.ArrayElement([.. SeededOrganisations]);

        var uri = new Uri(
            $"/users?organisationId={sampleOrganisation.Id}&pageSize={100}",
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

        var expectedIds = _seededUsers
            .Where(u =>
                u.UserOrgMemberships!.Select(m => m.OrganisationId).Contains(sampleOrganisation.Id)
            )
            .Select(x => x.Id);
        dto.Items.Select(i => i.UserId).ShouldBe(expectedIds, ignoreOrder: true);
    }

    [Fact]
    public async Task GetUsers_StatusQueryParametersProvided_FiltersByStatus()
    {
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uri = new Uri("/users?status=Active&status=Inactive", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        PaginatedResponseDto<UserListItemDto>? dto = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default, TestContext.Current.CancellationToken);
        dto.ShouldNotBeNull();
        dto.Items.ShouldContain(x => x.Status == UserOrgStatus.Active);
        dto.Items.ShouldContain(x => x.Status == UserOrgStatus.Inactive);
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
