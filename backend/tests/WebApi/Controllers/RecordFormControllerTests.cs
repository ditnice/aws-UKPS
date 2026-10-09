using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;
using UKPS.Api.Application.Forms.Rules;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Authentication;
using GetRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Forms.Dtos.RecordPageDto,
    UKPS.Api.Application.Forms.Errors.GetRecordPageError
>;
using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Forms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.Forms.Errors.SaveRecordPageError
>;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordFormControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Uri _pageUrl = new(
        "/records/1/revisions/2/pages/cancer",
        UriKind.Relative
    );

    private readonly IRecordPageQueryService _mockQueryService =
        Substitute.For<IRecordPageQueryService>();
    private readonly IRecordPageSaveService _mockSaveService =
        Substitute.For<IRecordPageSaveService>();
    private readonly HttpClient _client;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public RecordFormControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRecordPageQueryService>();
                    services.AddSingleton(_mockQueryService);
                    services.RemoveAll<IRecordPageSaveService>();
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

    private static RecordPageDto CreatePage() =>
        new()
        {
            FormVersion = "2026.10.1",
            RevisionVersion = 4711,
            ReadOnly = false,
            OrganisationId = 3,
            Section = new FormSectionDto
            {
                Id = "indication-details",
                Title = "Indication details",
            },
            Page = new FormPageDto { Id = "cancer", Title = "Treating cancer" },
            PreviousPageId = "paediatric",
            Questions =
            [
                new FormQuestionDto
                {
                    Id = "medicines_product_detail.indication_is_cancer",
                    Type = QuestionType.Radio,
                    Label = "Is this a product intended to treat cancer?",
                    Rules = [new FormRuleDto { Kind = RuleKind.Required, Message = "Select one" }],
                    Options = [new FormOptionDto { Value = "Yes", Label = "Yes" }],
                },
            ],
            Answers = new Dictionary<string, JsonNode?>(StringComparer.Ordinal)
            {
                ["medicines_product_detail.indication_is_cancer"] = JsonValue.Create("Yes"),
                ["medicines_product_detail_therapeutic_area"] = new JsonArray("4", "7"),
                ["medicines_product_detail.indication"] = null,
            },
            Context = new Dictionary<string, JsonNode?>(StringComparer.Ordinal),
        };

    [Fact]
    public async Task GetRecordPage_ReturnsThePageAsJson()
    {
        _mockQueryService
            .GetPage(1, 2, "cancer", Arg.Any<CancellationToken>())
            .Returns(GetRecordPageResult.Ok(CreatePage()));

        var response = await _client.GetAsync(_pageUrl, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(Ct))!;
        json["formVersion"]!.GetValue<string>().ShouldBe("2026.10.1");
        json["revisionVersion"]!.GetValue<uint>().ShouldBe(4711u);
        json["previousPageId"]!.GetValue<string>().ShouldBe("paediatric");

        var question = json["questions"]![0]!;
        question["type"]!.GetValue<string>().ShouldBe("Radio");
        question["rules"]![0]!["kind"]!.GetValue<string>().ShouldBe("Required");

        // Question IDs are used as answer keys verbatim, not camel-cased.
        var answers = json["answers"]!.AsObject();
        answers["medicines_product_detail.indication_is_cancer"]!
            .GetValue<string>()
            .ShouldBe("Yes");
        answers["medicines_product_detail_therapeutic_area"]!
            .ToJsonString()
            .ShouldBe("[\"4\",\"7\"]");
        answers.ContainsKey("medicines_product_detail.indication").ShouldBeTrue();
        answers["medicines_product_detail.indication"].ShouldBeNull();
    }

    public static TheoryData<string> ErrorCases =>
        new() { "RecordNotFound", "NotAllowed", "PageNotFound" };

    [Theory]
    [MemberData(nameof(ErrorCases))]
    public async Task GetRecordPage_AnyError_IsANeutral404(string errorCase)
    {
        GetRecordPageError error = errorCase switch
        {
            "RecordNotFound" => new GetRecordPageError.RecordNotFound(),
            "NotAllowed" => new GetRecordPageError.NotAllowed(),
            _ => new GetRecordPageError.PageNotFound(),
        };
        _mockQueryService
            .GetPage(1, 2, "cancer", Arg.Any<CancellationToken>())
            .Returns(GetRecordPageResult.Err(error));

        var response = await _client.GetAsync(_pageUrl, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(Ct))!;
        json["detail"]!.GetValue<string>().ShouldBe("The specified page could not be found.");
    }

    private const string SaveBody =
        """{"formVersion":"2026.10.1","revisionVersion":4711,"answers":{"medicines_product_detail.indication_is_cancer":"Yes","medicines_product_detail.indication":null}}""";

    private void SaveReturns(SaveRecordPageResult result) =>
        _mockSaveService
            .SavePage(
                1,
                2,
                "cancer",
                Arg.Any<SaveRecordPageCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(result);

    private async Task<(HttpStatusCode Status, JsonNode Body)> PutPage()
    {
        using var content = new StringContent(
            SaveBody,
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var response = await _client.PutAsync(_pageUrl, content, Ct);
        return (response.StatusCode, JsonNode.Parse(await response.Content.ReadAsStringAsync(Ct))!);
    }

    [Fact]
    public async Task SaveRecordPage_PassesTheAnswersThroughAndReturnsTheNextPage()
    {
        SaveReturns(SaveRecordPageResult.Ok(new SaveRecordPageDto { NextPageId = null }));

        var (status, body) = await PutPage();

        status.ShouldBe(HttpStatusCode.OK);
        body.AsObject().ContainsKey("nextPageId").ShouldBeTrue();
        body["nextPageId"].ShouldBeNull();
        await _mockSaveService
            .Received(1)
            .SavePage(
                1,
                2,
                "cancer",
                Arg.Is<SaveRecordPageCommand>(c =>
                    c.FormVersion == "2026.10.1"
                    && c.RevisionVersion == 4711
                    && c.Answers.Count == 2
                    && c.Answers[
                        "medicines_product_detail.indication_is_cancer"
                    ]!.GetValue<string>() == "Yes"
                    && c.Answers["medicines_product_detail.indication"] == null
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task SaveRecordPage_Invalid_ReturnsErrorsKeyedByQuestionId()
    {
        SaveReturns(
            SaveRecordPageResult.Err(
                new SaveRecordPageError.Invalid(
                    new Dictionary<string, string[]>(StringComparer.Ordinal)
                    {
                        ["medicines_product_detail.indication_is_cancer"] = ["Select one"],
                    }
                )
            )
        );

        var (status, body) = await PutPage();

        status.ShouldBe(HttpStatusCode.BadRequest);
        var errors = body["errors"]!.AsObject();
        errors.Select(x => x.Key).ShouldBe(["medicines_product_detail.indication_is_cancer"]);
        errors["medicines_product_detail.indication_is_cancer"]![0]!
            .GetValue<string>()
            .ShouldBe("Select one");
    }

    [Fact]
    public async Task SaveRecordPage_EditNotAllowed_Is403()
    {
        SaveReturns(SaveRecordPageResult.Err(new SaveRecordPageError.EditNotAllowed()));

        (await PutPage()).Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string> SaveNotFoundCases =>
        new() { "RecordNotFound", "NotAllowed", "PageNotFound" };

    [Theory]
    [MemberData(nameof(SaveNotFoundCases))]
    public async Task SaveRecordPage_NotFoundOrNotReadable_IsANeutral404(string errorCase)
    {
        SaveRecordPageError error = errorCase switch
        {
            "RecordNotFound" => new SaveRecordPageError.RecordNotFound(),
            "NotAllowed" => new SaveRecordPageError.NotAllowed(),
            _ => new SaveRecordPageError.PageNotFound(),
        };
        SaveReturns(SaveRecordPageResult.Err(error));

        var (status, body) = await PutPage();

        status.ShouldBe(HttpStatusCode.NotFound);
        body["detail"]!.GetValue<string>().ShouldBe("The specified page could not be found.");
    }

    public static TheoryData<string, string> ConflictCases =>
        new()
        {
            { "RevisionNotDraft", "revision_not_draft" },
            { "RevisionChanged", "revision_changed" },
            { "FormVersionChanged", "form_version_changed" },
        };

    [Theory]
    [MemberData(nameof(ConflictCases))]
    public async Task SaveRecordPage_Conflict_Is409WithACode(string errorCase, string expectedCode)
    {
        SaveRecordPageError error = errorCase switch
        {
            "RevisionNotDraft" => new SaveRecordPageError.RevisionNotDraft(),
            "RevisionChanged" => new SaveRecordPageError.RevisionChanged(),
            _ => new SaveRecordPageError.FormVersionChanged(),
        };
        SaveReturns(SaveRecordPageResult.Err(error));

        var (status, body) = await PutPage();

        status.ShouldBe(HttpStatusCode.Conflict);
        body["code"]!.GetValue<string>().ShouldBe(expectedCode);
    }
}
