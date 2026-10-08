using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Common;
using UKPS.Api.Tests.Application.Users;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class UserEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly Faker _faker = new Faker();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();
    private readonly IReadOnlyCollection<User> _seededUsers;
    private readonly List<UserRegistrationRequest> _userRegistrationRequests;

    private IEnumerable<User> ViewableUsers => _seededUsers;

    public UserEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(431); // Deterministic Randomisation

        _httpClient = fixture.Factory.CreateClient();

        var organisations = _organisationFaker.Generate(4);
        var userFaker = new UserFaker().RuleFor(
            x => x.UserOrgMemberships,
            (f, u) =>
            {
                return f.PickRandom(
                        organisations,
                        f.Random.Int(min: 1, max: Math.Min(3, organisations.Count))
                    )
                    .Select(o =>
                        _userOrgMembershipFaker.RuleFor(x => x.Organisation, _ => o).Generate()
                    )
                    .ToArray();
            }
        );
        _seededUsers = userFaker.Generate(50);
        _userRegistrationRequests = new UserRegistrationRequestFaker()
            .RuleFor(x => x.Organisation, f => f.PickRandom(organisations))
            .Generate(20);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await AddEntities(_seededUsers, TestContext.Current.CancellationToken);
        await AddEntities(_userRegistrationRequests, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetUsers_OrganisationIdProvided_ReturnsOnlyThatOrganisationsUsers()
    {
        var sampleUser = _faker.PickRandom(ViewableUsers);
        var sampleUserOrgs = sampleUser.UserOrgMemberships!.Select(x => x.OrganisationId);
        var userFromOtherOrganisation = _faker.PickRandom(
            ViewableUsers.Where(vu =>
            {
                var otherUserOrgs = vu.UserOrgMemberships!.Select(x => x.OrganisationId);
                return !otherUserOrgs.Intersect(sampleUserOrgs).Any();
            })
        );
        var randomOrganisation = _faker.PickRandom(sampleUserOrgs);

        var uri = new Uri(
            $"/users?organisationId={randomOrganisation}&pageSize={100}",
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
        dto.Items.Select(i => i.UserId).ShouldContain(sampleUser.Id);
        dto.Items.Select(i => i.UserId).ShouldNotContain(userFromOtherOrganisation.Id);
    }

    [Fact]
    public async Task GetUsers_StatusQueryParametersProvided_FiltersByStatus()
    {
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
        dto.Items.Select(x => x.Status)
            .ShouldOnlyContain([UserOrgStatus.Active, UserOrgStatus.Inactive]);
        dto.Items.Select(x => x.Status)
            .ShouldContainSet([UserOrgStatus.Active, UserOrgStatus.Inactive]);
        var returnedUsers = dto.Items.Select(x => _seededUsers.First(su => su.Id == x.UserId));

        foreach (var ru in returnedUsers)
        {
            ru.UserOrgMemberships!.Select(x => x.OrganisationId).ShouldContain(1);
        }
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

    [Fact]
    public async Task UpdateUserDetails_AnotherUserOrMissingUser_ReturnIndistinguishableNotFound()
    {
        User otherUser = _seededUsers.First();
        AuthenticateAs(UserRole.Standard, otherUser.UserOrgMemberships!.First().OrganisationId);
        UpdateUserDetailsCommand command = new UpdateUserDetailsCommandFaker().Generate();

        HttpResponseMessage otherUserResponse = await _httpClient.PatchAsJsonAsync(
            new Uri($"/users/{otherUser.Id}", UriKind.Relative),
            command,
            TestContext.Current.CancellationToken
        );
        HttpResponseMessage missingUserResponse = await _httpClient.PatchAsJsonAsync(
            new Uri("/users/999999", UriKind.Relative),
            command,
            TestContext.Current.CancellationToken
        );

        otherUserResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missingUserResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? otherUserProblem =
            await otherUserResponse.Content.ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken
            );
        ProblemDetails? missingUserProblem =
            await missingUserResponse.Content.ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken
            );
        otherUserProblem.ShouldNotBeNull();
        missingUserProblem.ShouldNotBeNull();
        otherUserProblem.Title.ShouldBe(missingUserProblem.Title);
        otherUserProblem.Detail.ShouldBe(missingUserProblem.Detail);
    }

    [Fact]
    public async Task GetUserRegistrationById_NotAuthorisedForOrganisation_ReturnsForbiddenWhetherOrNotRequestExists()
    {
        UserRegistrationRequest request = _userRegistrationRequests[0];
        AuthenticateAs(UserRole.Champion, request.OrganisationId + 1000);

        using HttpResponseMessage existingResponse = await SendMembershipRequest(
            "GET",
            "",
            request.OrganisationId,
            request.RequestGuid
        );
        using HttpResponseMessage guessedResponse = await SendMembershipRequest(
            "GET",
            "",
            request.OrganisationId,
            Guid.NewGuid()
        );

        existingResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        guessedResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserRegistrationById_Authorised_ReturnsRequestGuid()
    {
        UserRegistrationRequest request = _userRegistrationRequests[0];
        AuthenticateAs(UserRole.Champion, request.OrganisationId);

        using HttpResponseMessage response = await SendMembershipRequest(
            "GET",
            "",
            request.OrganisationId,
            request.RequestGuid
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<RegisterUserConfirmationDto>(
            TestContext.Current.CancellationToken
        );
        dto.ShouldNotBeNull().RequestGuid.ShouldBe(request.RequestGuid);
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("PATCH", "/approve")]
    [InlineData("PATCH", "/reject")]
    public async Task MembershipRequest_GuidFromAnotherOrganisation_ReturnsNotFound(
        string method,
        string action
    )
    {
        UserRegistrationRequest request = _userRegistrationRequests[0];
        int otherOrganisationId = _userRegistrationRequests
            .First(x => x.OrganisationId != request.OrganisationId)
            .OrganisationId;
        AuthenticateAs(UserRole.Champion, otherOrganisationId);

        using HttpResponseMessage response = await SendMembershipRequest(
            method,
            action,
            otherOrganisationId,
            request.RequestGuid
        );
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<HttpResponseMessage> SendMembershipRequest(
        string method,
        string action,
        int organisationId,
        Guid requestGuid
    )
    {
        using var message = new HttpRequestMessage(
            new HttpMethod(method),
            $"/organisations/{organisationId}/membership-requests/{requestGuid}{action}"
        );
        return await _httpClient.SendAsync(message, TestContext.Current.CancellationToken);
    }
}
