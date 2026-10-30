using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.FormAnnotations;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using GetRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.RecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.GetRecordPageError
>;
using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.SaveRecordPageError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordFormControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Indication = "medicines_indication_detail.indication";
    private const string PageUrl = "/records/5/pages/indication";

    private static readonly Dictionary<string, SaveRecordPageError> _conflicts = new(
        StringComparer.Ordinal
    )
    {
        ["revision_not_draft"] = new SaveRecordPageError.NotDraft(),
        ["revision_changed"] = new SaveRecordPageError.RevisionChanged(),
        ["form_version_changed"] = new SaveRecordPageError.FormVersionChanged(),
    };

    private readonly IRecordFormQueryService _mockQueryService =
        Substitute.For<IRecordFormQueryService>();
    private readonly IRecordFormSaveService _mockSaveService =
        Substitute.For<IRecordFormSaveService>();
    private readonly HttpClient _client;

    public RecordFormControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRecordFormQueryService>();
                    services.AddSingleton(_mockQueryService);
                    services.RemoveAll<IRecordFormSaveService>();
                    services.AddSingleton(_mockSaveService);
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
    public async Task GetRecordFormPage_ReturnsThePage()
    {
        RecordPageDto page = CreatePage();
        _mockQueryService
            .GetPage(5, "indication", Arg.Any<CancellationToken>())
            .Returns(GetRecordPageResult.Ok(page));

        HttpResponseMessage response = await _client.GetAsync(Url(PageUrl), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonObject body = (await response.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        body["formVersion"]!.GetValue<string>().ShouldBe(page.FormVersion);
        body["task"]!["title"]!.GetValue<string>().ShouldBe("Indication details");
        body["questions"]![0]!["type"]!.GetValue<string>().ShouldBe("Textarea");
        body["questions"]![0]!["rules"]![0]!["kind"]!.GetValue<string>().ShouldBe("Required");
        body["answers"]![Indication]!.GetValue<string>().ShouldBe("Hepatitis C");
    }

    [Fact]
    public async Task GetRecordFormPage_InaccessibleAndMissingPages_ReturnIdenticalResponses()
    {
        _mockQueryService
            .GetPage(5, "indication", Arg.Any<CancellationToken>())
            .Returns(GetRecordPageResult.Err(new GetRecordPageError.NotAllowed()));
        _mockQueryService
            .GetPage(5, "unknown", Arg.Any<CancellationToken>())
            .Returns(GetRecordPageResult.Err(new GetRecordPageError.NotFound()));

        await ShouldBeIdenticalNotFound(
            await _client.GetAsync(Url(PageUrl), Ct),
            await _client.GetAsync(Url("/records/5/pages/unknown"), Ct)
        );
    }

    [Fact]
    public async Task SaveRecordFormPage_ReturnsTheNextPageAndPassesTheCommandOn()
    {
        _mockSaveService
            .SavePage(
                5,
                "indication",
                Arg.Any<SaveRecordPageCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(SaveRecordPageResult.Ok(new SaveRecordPageDto { NextPageId = "bnf-chapter" }));

        HttpResponseMessage response = await Save(PageUrl);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<SaveRecordPageDto>(Ct))!.NextPageId.ShouldBe(
            "bnf-chapter"
        );
        await _mockSaveService
            .Received(1)
            .SavePage(
                5,
                "indication",
                Arg.Is<SaveRecordPageCommand>(c =>
                    c.FormVersion == "2026.10.4"
                    && c.RevisionVersion == 42
                    && c.Answers[Indication].GetString() == "Hepatitis C"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task SaveRecordFormPage_InvalidAnswers_ReturnsValidationProblemKeyedByQuestionId()
    {
        SetSaveResult(
            new SaveRecordPageError.Invalid(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [Indication] = "Enter the indication",
                    ["stray"] = "This is not a question on this page.",
                }
            )
        );

        HttpResponseMessage response = await Save(PageUrl);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ValidationProblemDetails problem = (
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct)
        )!;
        problem.Errors[Indication].ShouldBe(["Enter the indication"]);
        problem.Errors["stray"].ShouldBe(["This is not a question on this page."]);
    }

    [Fact]
    public async Task SaveRecordFormPage_MissingAnswers_IsRejectedBeforeTheService()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            Url(PageUrl),
            new { formVersion = "2026.10.4", revisionVersion = 42 },
            Ct
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await _mockSaveService.DidNotReceiveWithAnyArgs().SavePage(default, default!, default!, Ct);
    }

    [Fact]
    public async Task SaveRecordFormPage_CannotEdit_ReturnsForbidden()
    {
        SetSaveResult(new SaveRecordPageError.CannotEdit());

        HttpResponseMessage response = await Save(PageUrl);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SaveRecordFormPage_InaccessibleAndMissingPages_ReturnIdenticalResponses()
    {
        SetSaveResult(new SaveRecordPageError.NotAllowed(), "indication");
        SetSaveResult(new SaveRecordPageError.NotFound(), "unknown");

        await ShouldBeIdenticalNotFound(
            await Save(PageUrl),
            await Save("/records/5/pages/unknown")
        );
    }

    [Theory]
    [InlineData("revision_not_draft")]
    [InlineData("revision_changed")]
    [InlineData("form_version_changed")]
    public async Task SaveRecordFormPage_Conflict_ReturnsConflictWithCode(string code)
    {
        SetSaveResult(_conflicts[code]);

        HttpResponseMessage response = await Save(PageUrl);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        JsonObject body = (await response.Content.ReadFromJsonAsync<JsonObject>(Ct))!;
        body["code"]!.GetValue<string>().ShouldBe(code);
        body["detail"]!.GetValue<string>().ShouldNotBeNullOrWhiteSpace();
    }

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private Task<HttpResponseMessage> Save(string path) =>
        _client.PutAsJsonAsync(
            Url(path),
            new SaveRecordPageCommand
            {
                FormVersion = "2026.10.4",
                RevisionVersion = 42,
                Answers = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
                {
                    [Indication] = JsonSerializer.SerializeToElement("Hepatitis C"),
                },
            },
            Ct
        );

    private void SetSaveResult(SaveRecordPageError error, string pageId = "indication") =>
        _mockSaveService
            .SavePage(5, pageId, Arg.Any<SaveRecordPageCommand>(), Arg.Any<CancellationToken>())
            .Returns(SaveRecordPageResult.Err(error));

    private static async Task ShouldBeIdenticalNotFound(
        HttpResponseMessage inaccessible,
        HttpResponseMessage missing
    )
    {
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

    private static RecordPageDto CreatePage() =>
        new()
        {
            FormVersion = "2026.10.4",
            RevisionVersion = 42,
            RecordId = 5,
            Section = new RecordPageSectionDto
            {
                Id = "indication-details",
                Title = "Indication details",
            },
            Task = new RecordPageTaskDto
            {
                Id = "indication-details",
                Title = "Indication details",
            },
            Page = new RecordPagePageDto { Id = "indication", Title = "Indication" },
            PreviousPageId = null,
            NextPageId = "bnf-chapter",
            Questions =
            [
                new RecordPageQuestionDto
                {
                    Id = Indication,
                    Type = FormControl.Textarea,
                    Multiple = false,
                    Label = "What is the indication this product is seeking a licence for?",
                    Hint = null,
                    Display = null,
                    Placeholder = null,
                    ChildLabel = null,
                    ChildPlaceholder = null,
                    AddAnotherLabel = null,
                    Rules =
                    [
                        new RecordPageRuleDto
                        {
                            Kind = RuleKind.Required,
                            Value = null,
                            Message = "Enter the indication",
                        },
                    ],
                    Options = null,
                },
            ],
            Answers = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                [Indication] = JsonSerializer.SerializeToElement("Hepatitis C"),
            },
        };
}
