using System.Net;
using System.Net.Http.Json;
using System.Text;
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
using SubmitRecordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.RecordForms.Errors.SubmitRecordError>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordSubmissionControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IRecordSubmissionService _service = Substitute.For<IRecordSubmissionService>();
    private readonly HttpClient _client;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly Uri _url = new("/records/5/submit", UriKind.Relative);

    public RecordSubmissionControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRecordSubmissionService>();
                    services.AddSingleton(_service);
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

    [Fact]
    public async Task Submit_Success_ReturnsNoContentAndPassesCommand()
    {
        _service
            .Submit(5, Arg.Any<SubmitRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(SubmitRecordResult.Ok());

        (await Submit()).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        await _service
            .Received(1)
            .Submit(
                5,
                Arg.Is<SubmitRecordCommand>(c =>
                    c.FormVersion == "version" && c.RevisionHash == "hash"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Submit_InaccessibleAndMissing_ReturnIdenticalNotFound()
    {
        SetError(new SubmitRecordError.NotAllowed());
        var inaccessible = await Submit();
        SetError(new SubmitRecordError.NotFound());
        var missing = await Submit();
        inaccessible.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var first = (await inaccessible.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        var second = (await missing.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        first.Remove("traceId");
        second.Remove("traceId");
        JsonNode.DeepEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public async Task Submit_CannotSubmit_ReturnsForbidden()
    {
        SetError(new SubmitRecordError.CannotSubmit());
        (await Submit()).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("record_not_submittable")]
    [InlineData("form_version_changed")]
    [InlineData("record_changed")]
    public async Task Submit_Conflict_ReturnsCodeAndDetail(string code)
    {
        SetError(
            code switch
            {
                "record_not_submittable" => new SubmitRecordError.NotSubmittable(),
                "form_version_changed" => new SubmitRecordError.FormVersionChanged(),
                _ => new SubmitRecordError.RecordChanged(),
            }
        );
        var response = await Submit();
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var body = (await response.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        body["code"]!.GetValue<string>().ShouldBe(code);
        body["detail"]!.GetValue<string>().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Submit_Invalid_ReturnsKeyedErrorsAndOrderedLocations()
    {
        SubmissionErrorDto[] fields =
        [
            new()
            {
                QuestionId = "first",
                PageId = "page-one",
                TaskId = "task-one",
                Message = "Fix first",
            },
            new()
            {
                QuestionId = "second",
                PageId = "page-two",
                TaskId = "task-two",
                Message = "Fix second",
            },
        ];
        SetError(new SubmitRecordError.Invalid(fields));
        var response = await Submit();
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var body = (await response.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        body["code"]!.GetValue<string>().ShouldBe("record_invalid");
        body["errors"]!["first"]![0]!.GetValue<string>().ShouldBe("Fix first");
        body["errors"]!["second"]![0]!.GetValue<string>().ShouldBe("Fix second");
        for (int i = 0; i < fields.Length; i++)
        {
            body["fields"]![i]!["questionId"]!.GetValue<string>().ShouldBe(fields[i].QuestionId);
            body["fields"]![i]!["pageId"]!.GetValue<string>().ShouldBe(fields[i].PageId);
            body["fields"]![i]!["taskId"]!.GetValue<string>().ShouldBe(fields[i].TaskId);
            body["fields"]![i]!["message"]!.GetValue<string>().ShouldBe(fields[i].Message);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{")]
    [InlineData("{}")]
    [InlineData("{\"formVersion\":\"version\"}")]
    [InlineData("{\"revisionHash\":\"hash\"}")]
    [InlineData("{\"formVersion\":null,\"revisionHash\":\"hash\"}")]
    [InlineData("{\"formVersion\":\"version\",\"revisionHash\":null}")]
    [InlineData("{\"formVersion\":\" \",\"revisionHash\":\"hash\"}")]
    [InlineData("{\"formVersion\":\"version\",\"revisionHash\":\" \"}")]
    [InlineData("{\"formVersion\":\"\",\"revisionHash\":\"hash\"}")]
    [InlineData("{\"formVersion\":\"version\",\"revisionHash\":\"\"}")]
    public async Task Submit_MalformedCommand_IsRejectedBeforeService(string? json)
    {
        // Test a missing JSON body independently of media-type negotiation.
        using var content = new StringContent(json ?? "", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_url, content, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await _service.DidNotReceiveWithAnyArgs().Submit(default, default!, Ct);
    }

    private Task<HttpResponseMessage> Submit() =>
        _client.PostAsJsonAsync(
            _url,
            new SubmitRecordCommand { FormVersion = "version", RevisionHash = "hash" },
            Ct
        );

    private void SetError(SubmitRecordError error) =>
        _service
            .Submit(5, Arg.Any<SubmitRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(SubmitRecordResult.Err(error));
}
