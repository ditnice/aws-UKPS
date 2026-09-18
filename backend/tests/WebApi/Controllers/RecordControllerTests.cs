using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using SortDirection = UKPS.Api.Application.Common.SortDirection;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string RecordsUrl = "/records";
    private readonly IRecordService _mockRecordService = Substitute.For<IRecordService>();
    private readonly HttpClient _client;

    public RecordControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRecordService>();
                    services.AddSingleton(_mockRecordService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        _mockRecordService
            .GetRecords(Arg.Any<GetRecordsQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>.Ok(
                    CreatePaginatedResponse()
                )
            );
    }

    [Fact]
    public async Task GetRecords_ReturnsOk_WhenRecordsExist()
    {
        PaginatedResponseDto<RecordListItemDto> expected = CreatePaginatedResponse();
        _mockRecordService
            .GetRecords(Arg.Any<GetRecordsQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>.Ok(expected));

        var url = AppendQueryParams(RecordsUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<
            PaginatedResponseDto<RecordListItemDto>
        >(TestJsonOptions.Default, TestContext.Current.CancellationToken);

        content.ShouldNotBeNull();
        ShouldBeEquivalentTo(expected, content);
    }

    [Fact]
    public async Task GetRecords_ReturnsBadRequest_WhenOrganisationNotFound()
    {
        _mockRecordService
            .GetRecords(Arg.Any<GetRecordsQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>.Err(
                    new GetRecordsError.OrganisationNotFound(1)
                )
            );

        var url = AppendQueryParams(RecordsUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        content.ShouldBe("Organisation not found.");
    }

    [Fact]
    public async Task GetRecords_ReturnsForbidden_WhenNotAllowed()
    {
        _mockRecordService
            .GetRecords(Arg.Any<GetRecordsQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>.Err(
                    new GetRecordsError.NotAllowed(1)
                )
            );

        var url = AppendQueryParams(RecordsUrl, CreateQuery());
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRecords_PassesQueryValuesToService()
    {
        _mockRecordService
            .GetRecords(Arg.Any<GetRecordsQueryDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError>.Ok(
                    CreatePaginatedResponse()
                )
            );

        GetRecordsQueryDtoFaker faker = new();
        foreach (var _ in Enumerable.Range(0, 50))
        {
            _mockRecordService.ClearReceivedCalls();
            var query = faker.Generate();
            var url = AppendQueryParams(RecordsUrl, query);
            await _client.GetAsync(url, TestContext.Current.CancellationToken);

            await _mockRecordService
                .Received(1)
                .GetRecords(
                    Arg.Do<GetRecordsQueryDto>(x => x.ShouldBeEquivalentTo(query)),
                    Arg.Any<CancellationToken>()
                );
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetRecords_ReturnsBadRequest_WhenPageIsLessThanOne(int page)
    {
        var query = CreateQuery() with { Page = page };
        var url = AppendQueryParams(RecordsUrl, query);
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetRecords_ReturnsBadRequest_WhenPageSizeIsOutsideAllowedRange(int pageSize)
    {
        var query = CreateQuery() with { PageSize = pageSize };
        var url = AppendQueryParams(RecordsUrl, query);
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static Uri AppendQueryParams(string url, GetRecordsQueryDto query)
    {
        var queryParams = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}",
            $"sortBy={Uri.EscapeDataString(query.SortBy.ToString())}",
            $"sortDirection={Uri.EscapeDataString(query.SortDirection.ToString())}",
        };

        foreach (var status in query.RecordStatus)
            queryParams.Add($"recordStatus={Uri.EscapeDataString(status.ToString())}");

        foreach (var type in query.RecordType)
            queryParams.Add($"recordType={Uri.EscapeDataString(type.ToString())}");

        if (query.UpdateStatus.HasValue)
            queryParams.Add(
                $"updateStatus={Uri.EscapeDataString(query.UpdateStatus.Value.ToString())}"
            );

        if (!string.IsNullOrWhiteSpace(query.Search))
            queryParams.Add($"search={Uri.EscapeDataString(query.Search)}");

        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return new Uri(
            $"{url}{separator}{string.Join("&", queryParams)}",
            UriKind.RelativeOrAbsolute
        );
    }

    private static GetRecordsQueryDto CreateQuery() =>
        new()
        {
            Page = 1,
            PageSize = 20,
            SortBy = GetRecordsQuerySortValue.Id,
            SortDirection = SortDirection.Ascending,
        };

    private static PaginatedResponseDto<RecordListItemDto> CreatePaginatedResponse() =>
        new()
        {
            Items =
            [
                new RecordListItemDto
                {
                    Id = 1,
                    RecordType = RecordType.Medicine,
                    RecordStatus = RecordStatus.Active,
                    Title = "Test Record",
                    NiceTaDevelopmentId = null,
                    ReviewedAt = null,
                },
            ],
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
        };

    private static void ShouldBeEquivalentTo(
        PaginatedResponseDto<RecordListItemDto> expected,
        PaginatedResponseDto<RecordListItemDto> actual
    )
    {
        actual.TotalCount.ShouldBe(expected.TotalCount);
        actual.Page.ShouldBe(expected.Page);
        actual.PageSize.ShouldBe(expected.PageSize);
        actual.Items.Count.ShouldBe(expected.Items.Count);
        foreach (var (expectedItem, actualItem) in expected.Items.Zip(actual.Items))
        {
            actualItem.Id.ShouldBe(expectedItem.Id);
            actualItem.RecordType.ShouldBe(expectedItem.RecordType);
            actualItem.RecordStatus.ShouldBe(expectedItem.RecordStatus);
            actualItem.Title.ShouldBe(expectedItem.Title);
            actualItem.NiceTaDevelopmentId.ShouldBe(expectedItem.NiceTaDevelopmentId);
            actualItem.ReviewedAt.ShouldBe(expectedItem.ReviewedAt);
        }
    }

    private sealed class GetRecordsQueryDtoFaker : Faker<GetRecordsQueryDto>
    {
        public GetRecordsQueryDtoFaker()
        {
            RuleFor(x => x.Page, f => f.Random.Int(1, 10));
            RuleFor(x => x.PageSize, f => f.Random.Int(1, 100));
            RuleFor(x => x.SortBy, f => f.PickRandom<GetRecordsQuerySortValue>());
            RuleFor(x => x.SortDirection, f => f.PickRandom<SortDirection>());
            RuleFor(
                x => x.RecordStatus,
                f => f.Make(f.Random.Int(0, 3), () => f.PickRandom<RecordStatus>())
            );
            RuleFor(
                x => x.RecordType,
                f => f.Make(f.Random.Int(0, 3), () => f.PickRandom<RecordType>())
            );
            RuleFor(
                x => x.UpdateStatus,
                f => f.Random.Bool(0.5f) ? f.PickRandom<UpdateStatus>() : null
            );
            RuleFor(x => x.Search, f => f.Random.Bool(0.5f) ? f.Lorem.Word() : null);
        }
    }
}
