using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using GetRecordTaskListResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.RecordTaskListDto,
    UKPS.Api.Application.RecordForms.Errors.GetRecordTaskListError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordTaskListControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TaskListUrl = "/records/5/tasks";

    private readonly IRecordTaskListQueryService _mockQueryService =
        Substitute.For<IRecordTaskListQueryService>();
    private readonly HttpClient _client;

    public RecordTaskListControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRecordTaskListQueryService>();
                    services.AddSingleton(_mockQueryService);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationOptions.SectionName}:{nameof(DevAuthenticationOptions.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetRecordTaskList_ReturnsTheTaskList()
    {
        _mockQueryService
            .GetTaskList(5, Arg.Any<CancellationToken>())
            .Returns(GetRecordTaskListResult.Ok(CreateTaskList()));

        HttpResponseMessage response = await _client.GetAsync(Url(TaskListUrl), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonObject body = (await response.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        body["recordId"]!.GetValue<int>().ShouldBe(5);
        body["organisationId"]!.GetValue<int>().ShouldBe(1);
        JsonNode section = body["sections"]![0]!;
        section["id"]!.GetValue<string>().ShouldBe("indication-details");
        JsonNode task = section["tasks"]![0]!;
        task["id"]!.GetValue<string>().ShouldBe("indication-details");
        task["title"]!.GetValue<string>().ShouldBe("Indication details");
        task["status"]!.GetValue<string>().ShouldBe("InProgress");
        task["firstPageId"]!.GetValue<string>().ShouldBe("indication");
    }

    [Fact]
    public async Task GetRecordTaskList_InaccessibleAndMissingRecords_ReturnIdenticalResponses()
    {
        _mockQueryService
            .GetTaskList(5, Arg.Any<CancellationToken>())
            .Returns(GetRecordTaskListResult.Err(new GetRecordTaskListError.NotAllowed()));
        _mockQueryService
            .GetTaskList(6, Arg.Any<CancellationToken>())
            .Returns(GetRecordTaskListResult.Err(new GetRecordTaskListError.NotFound()));

        HttpResponseMessage inaccessible = await _client.GetAsync(Url(TaskListUrl), Ct);
        HttpResponseMessage missing = await _client.GetAsync(Url("/records/6/tasks"), Ct);

        inaccessible.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        JsonObject inaccessibleBody = (
            await inaccessible.Content.ReadFromJsonAsync<JsonObject>(Ct)
        )!;
        JsonObject missingBody = (await missing.Content.ReadFromJsonAsync<JsonObject>(Ct))!;

        // Request-specific tracing metadata does not describe the resource's existence.
        inaccessibleBody.Remove("traceId");
        missingBody.Remove("traceId");
        JsonNode.DeepEquals(inaccessibleBody, missingBody).ShouldBeTrue();
    }

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private static RecordTaskListDto CreateTaskList() =>
        new()
        {
            RecordId = 5,
            OrganisationId = 1,
            Sections =
            [
                new RecordTaskListSectionDto
                {
                    Id = "indication-details",
                    Title = "Indication details",
                    Tasks =
                    [
                        new RecordTaskListTaskDto
                        {
                            Id = "indication-details",
                            Title = "Indication details",
                            Status = RecordTaskStatus.InProgress,
                            FirstPageId = "indication",
                        },
                    ],
                },
            ],
        };
}
