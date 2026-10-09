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

namespace UKPS.Api.Tests.WebApi.Controllers;

public class RecordFormControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Uri _pageUrl = new(
        "/records/1/revisions/2/pages/cancer",
        UriKind.Relative
    );

    private readonly IRecordPageQueryService _mockQueryService =
        Substitute.For<IRecordPageQueryService>();
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
}
