using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.Users;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string UsersUrl = "/users";
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly HttpClient _client;
    private readonly UpdateUserDetailsCommandFaker _updateUserDetailsCommandFaker = new();

    public UserControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserService>();
                    services.AddSingleton(_mockUserService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        PaginatedResponseDto<UserListItemDto> expected = CreatePaginatedResponse();
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(expected));

        _mockUserService
            .UpdateUserDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateUserDetailsCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<UserDetailsDto, UpdateUserDetailsError>.Ok(
                    new UserDetailsDtoFaker().Generate()
                )
            );
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUserFromTheUserService()
    {
        CurrentUserInformationDto expectedValue = new CurrentUserInformationDtoFaker().Generate();
        _mockUserService.GetCurrentUser(Arg.Any<CancellationToken>()).Returns(expectedValue);

        var url = new Uri($"{UsersUrl}/me", UriKind.Relative);
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<CurrentUserInformationDto>(
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );
        data.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task GetUsers_ReturnsOk_WhenOrganisationExists()
    {
        PaginatedResponseDto<UserListItemDto> expected = CreatePaginatedResponse();
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(expected));

        var url = AppendQueryParams(UsersUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<UserListItemDto>
        >(TestJsonOptions.Default, TestContext.Current.CancellationToken);

        content.ShouldNotBeNull();
        ShouldBeEquivalentTo(expected, content);
    }

    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );

        var url = AppendQueryParams(UsersUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        content.ShouldBe("Organisation not found.");
    }

    [Fact]
    public async Task GetUsers_ReturnsForbid_WhenNotAllowed()
    {
        var sampleId = 1;
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.NotAllowed(sampleId)
                )
            );

        var url = AppendQueryParams(UsersUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_PassesQueryValuesToService()
    {
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );
        GetUsersQueryDtoFaker faker = new GetUsersQueryDtoFaker();
        foreach (var _ in Enumerable.Range(0, 50))
        {
            _mockUserService.ClearReceivedCalls();
            var query = faker.Generate();
            var url = AppendQueryParams(UsersUrl, faker.Generate());
            await _client.GetAsync(url, TestContext.Current.CancellationToken);

            await _mockUserService
                .Received(1)
                .GetUsers(
                    Arg.Do<GetUsersQueryDto>(x => x.ShouldBeEquivalentTo(query)),
                    Arg.Any<CancellationToken>()
                );
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetUsers_ReturnsBadRequest_WhenPageIsLessThanOne(int page)
    {
        var query = CreateQuery() with { Page = page };
        var url = AppendQueryParams(UsersUrl, query);
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetUsers_ReturnsBadRequest_WhenPageSizeIsOutsideAllowedRange(int pageSize)
    {
        var query = CreateQuery() with { PageSize = pageSize };
        var url = AppendQueryParams(UsersUrl, query);
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenValidRequest_ShouldReturnOk()
    {
        UpdateUserDetailsCommandFaker faker = new();
        var url = new Uri($"{UsersUrl}/{1}", UriKind.Relative);
        var response = await _client.PatchAsJsonAsync(
            url,
            faker.Generate(),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenValidRequest_ShouldPassParametersToService()
    {
        var userId = 1;
        var command = _updateUserDetailsCommandFaker.Generate();
        var url = new Uri($"{UsersUrl}/{userId}", UriKind.Relative);
        _ = await _client.PatchAsJsonAsync(url, command, TestContext.Current.CancellationToken);

        await _mockUserService
            .Received()
            .UpdateUserDetails(userId, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserDetails_WhenForbidden_ShouldReturnForbiddenResponse()
    {
        _mockUserService
            .UpdateUserDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateUserDetailsCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<UserDetailsDto, UpdateUserDetailsError>.Err(
                    new UpdateUserDetailsError.Unauthorised()
                )
            );

        var url = new Uri($"{UsersUrl}/{1}", UriKind.Relative);
        var response = await _client.PatchAsJsonAsync(
            url,
            _updateUserDetailsCommandFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenUserDoesNotExist_ShouldReturnNotFoundResponse()
    {
        _mockUserService
            .UpdateUserDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateUserDetailsCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<UserDetailsDto, UpdateUserDetailsError>.Err(
                    new UpdateUserDetailsError.UserDoesNotExist()
                )
            );

        var url = new Uri($"{UsersUrl}/{1}", UriKind.Relative);
        var response = await _client.PatchAsJsonAsync(
            url,
            _updateUserDetailsCommandFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenNewEmailConflicts_ShouldReturnConflictResponse()
    {
        _mockUserService
            .UpdateUserDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateUserDetailsCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Result<UserDetailsDto, UpdateUserDetailsError>.Err(
                    new UpdateUserDetailsError.ConflictingEmail()
                )
            );

        var url = new Uri($"{UsersUrl}/{1}", UriKind.Relative);
        var response = await _client.PatchAsJsonAsync(
            url,
            _updateUserDetailsCommandFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenCommandIsInvalid_ShouldReturnBadRequestResponse()
    {
        Func<UpdateUserDetailsCommand, UpdateUserDetailsCommand>[] modifers =
        [
            x => x with { FullName = string.Empty },
            x => x with { FullName = null! },
            x => x with { WorkEmail = string.Empty },
            x => x with { WorkEmail = null! },
            x => x with { WorkEmail = "not a valid email" },
            x => x with { WorkTelephone = string.Empty },
            x => x with { WorkTelephone = null! },
        ];

        foreach (var mod in modifers)
        {
            var url = new Uri($"{UsersUrl}/{1}", UriKind.Relative);
            var response = await _client.PatchAsJsonAsync(
                url,
                mod(_updateUserDetailsCommandFaker.Generate()),
                TestContext.Current.CancellationToken
            );

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    private static Uri AppendQueryParams(string url, GetUsersQueryDto query)
    {
        var queryParams = new List<string>();

        if (query.OrganisationId.HasValue)
            queryParams.Add($"organisationId={query.OrganisationId.Value}");

        queryParams.Add($"page={query.Page}");
        queryParams.Add($"pageSize={query.PageSize}");

        foreach (var status in query.Status)
            queryParams.Add($"status={Uri.EscapeDataString(status.ToString())}");

        foreach (var role in query.Role)
            queryParams.Add($"role={Uri.EscapeDataString(role.ToString())}");

        if (!string.IsNullOrWhiteSpace(query.Email))
            queryParams.Add($"email={Uri.EscapeDataString(query.Email)}");

        if (query.LastActiveFrom.HasValue)
            queryParams.Add(
                $"lastActiveFrom={Uri.EscapeDataString(query.LastActiveFrom.Value.ToString("O"))}"
            );

        if (query.LastActiveTo.HasValue)
            queryParams.Add(
                $"lastActiveTo={Uri.EscapeDataString(query.LastActiveTo.Value.ToString("O"))}"
            );

        if (queryParams.Count == 0)
            return new Uri(url, UriKind.Relative);

        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';

        var output = $"{url}{separator}{string.Join("&", queryParams)}";

        return new Uri(output, UriKind.RelativeOrAbsolute);
    }

    private static GetUsersQueryDto CreateQuery() =>
        new()
        {
            OrganisationId = 1,
            Page = 1,
            PageSize = 20,
        };

    private static PaginatedResponseDto<UserListItemDto> CreatePaginatedResponse() =>
        new()
        {
            Items =
            [
                new UserListItemDto
                {
                    UserId = 1,
                    EmailAddress = "user@example.com",
                    Role = UserRole.Standard,
                    Status = UserOrgStatus.Active,
                },
            ],
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
        };

    private static void ShouldBeEquivalentTo<T>(
        PaginatedResponseDto<T> expected,
        PaginatedResponseDto<T> actual
    )
    {
        actual.TotalCount.ShouldBe(expected.TotalCount);
        actual.Page.ShouldBe(expected.Page);
        actual.PageSize.ShouldBe(expected.PageSize);
        actual.Items.ShouldBe(expected.Items);
    }

    private sealed class GetUsersQueryDtoFaker : Faker<GetUsersQueryDto>
    {
        public GetUsersQueryDtoFaker()
        {
            RuleFor(x => x.OrganisationId, f => f.Random.Bool(0.7f) ? f.Random.Int(1, 1000) : null);

            RuleFor(x => x.Page, f => f.Random.Int(1, 10));

            RuleFor(x => x.PageSize, f => f.Random.Int(1, 100));

            RuleFor(
                x => x.Status,
                f => f.Make(f.Random.Int(0, 3), () => f.PickRandom<UserOrgStatus>())
            );

            RuleFor(x => x.Role, f => f.Make(f.Random.Int(0, 3), () => f.PickRandom<UserRole>()));

            RuleFor(x => x.Email, f => f.Random.Bool(0.7f) ? f.Internet.Email() : null);

            RuleFor(
                x => x.LastActiveFrom,
                f =>
                    f.Random.Bool(0.5f)
                        ? new DateTimeOffset(
                            DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc),
                            TimeSpan.Zero
                        )
                        : null
            );

            RuleFor(
                x => x.LastActiveTo,
                (f, query) =>
                {
                    if (query.LastActiveFrom is null)
                    {
                        return f.Random.Bool(0.5f)
                            ? new DateTimeOffset(
                                DateTime.SpecifyKind(f.Date.Past(), DateTimeKind.Utc),
                                TimeSpan.Zero
                            )
                            : null;
                    }

                    return new DateTimeOffset(
                        f.Date.Between(query.LastActiveFrom.Value.DateTime, DateTime.UtcNow),
                        TimeSpan.Zero
                    );
                }
            );
        }
    }

    private sealed class UserDetailsDtoFaker : Faker<UserDetailsDto>
    {
        public UserDetailsDtoFaker()
        {
            RuleFor(x => x.UserType, f => f.PickRandom<UserType>());
            RuleFor(x => x.Title, f => f.PickRandom("Mr", "Mrs", "Ms", "Miss", "Dr", null));
            RuleFor(x => x.FullName, f => f.Name.FullName());
            RuleFor(x => x.JobTitle, f => f.Random.Bool() ? f.Name.JobTitle() : null);
            RuleFor(
                x => x.WorkPhone,
                f => f.Random.Bool() ? new TelephoneNumberFaker().Generate() : null
            );
            RuleFor(x => x.WorkEmail, f => f.Internet.Email());
        }
    }

    private sealed class CurrentUserInformationDtoFaker : Faker<CurrentUserInformationDto>
    {
        public CurrentUserInformationDtoFaker()
        {
            RuleFor(x => x.OrganisationMembershipId, f => f.Random.Int(1));
            RuleFor(x => x.OrganisationId, f => f.Random.Int(1));
            RuleFor(x => x.OrganisationName, f => f.Company.CompanyName());
            RuleFor(x => x.WorkEmail, f => f.Internet.Email());
            RuleFor(x => x.UserRole, f => f.PickRandom<UserRole>());
        }
    }
}
