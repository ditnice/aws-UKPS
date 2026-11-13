using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using UKPS.Api.WebApi.InternalServices.Identity;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class RecordSubmissionEndpointTests : DatabaseTestBase
{
    private const string IndicationIsCancer = "medicines_indication_detail.indication_is_cancer";

    private readonly HttpClient _httpClient;
    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;

    public RecordSubmissionEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _httpClient = fixture.Factory.CreateClient();
        Randomizer.Seed = new Random(576);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(new OrganisationFaker().Generate(), Ct);

        // The caller's Cognito username, so the submission is attributed to this user.
        string username = TestAuthenticationOptions
            .DefaultClaims.Single(c =>
                string.Equals(c.Type, UkpsClaimTypes.Username, StringComparison.Ordinal)
            )
            .Value;
        _user = await AddEntity(
            new UserFaker()
                .RuleFor(x => x.CognitoUsername, CognitoUsername.Parse(username))
                .Generate(),
            Ct
        );
        _record = await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, _organisation.Id)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, RecordStatus.Unpublished)
                .Generate(),
            Ct
        );
        _revision = await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
                .Generate(),
            Ct
        );
        // Far enough ahead that the estimate is never in the past on the real clock.
        await CompleteMedicineRevision.AddAsync(
            Context,
            _revision.Id,
            new DateOnly(2040, 1, 1),
            Ct
        );
        AuthenticateAs(UserRole.Standard, _organisation.Id);
    }

    [Fact]
    public async Task Submit_CompleteDraft_ThenGetRecord_ShowsItInQaReview()
    {
        using HttpResponseMessage response = await PostSubmit(await CurrentHash());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonObject dto = (
            await _httpClient.GetFromJsonAsync<JsonObject>(
                new Uri(
                    $"/records/{_record.Id}?recordType=Medicine&organisationId={_organisation.Id}",
                    UriKind.Relative
                ),
                Ct
            )
        ).ShouldNotBeNull();
        dto["displayStatus"]!.GetValue<string>().ShouldBe(nameof(RecordDisplayStatus.QAReview));
        dto["actions"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ShouldBe([nameof(RecordAction.ViewTimeline)]);

        RecordEvent recordEvent = await Context
            .RecordEvents.AsNoTracking()
            .SingleAsync(e => e.RecordId == _record.Id, Ct);
        recordEvent.EventType.ShouldBe(RecordEventType.SubmittedToQa);
        recordEvent.PerformedBy.ShouldBe(_user.Id);
    }

    [Fact]
    public async Task Submit_IncompleteDraft_ReturnsTheFieldErrors()
    {
        await Context
            .MedicinesIndicationDetails.Where(x => x.RevisionId == _revision.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.IndicationIsCancer, (YesNoUnknown?)null),
                Ct
            );

        using HttpResponseMessage response = await PostSubmit(await CurrentHash());

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        JsonObject problem = (
            await response.Content.ReadFromJsonAsync<JsonObject>(Ct)
        ).ShouldNotBeNull();
        problem["status"]!.GetValue<int>().ShouldBe(StatusCodes.Status422UnprocessableEntity);
        problem["code"]!.GetValue<string>().ShouldBe("record_invalid");
        problem["errors"]!.AsObject().Select(static e => e.Key).ShouldBe([IndicationIsCancer]);
        JsonObject field = problem["fields"]!.AsArray().ShouldHaveSingleItem()!.AsObject();
        field["questionId"]!.GetValue<string>().ShouldBe(IndicationIsCancer);
        field["pageId"]!.GetValue<string>().ShouldNotBeNullOrEmpty();
        field["taskId"]!.GetValue<string>().ShouldNotBeNullOrEmpty();
        field["message"]!.GetValue<string>().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Submit_StaleHash_ReturnsRecordChangedConflict()
    {
        using HttpResponseMessage response = await PostSubmit("stale");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        JsonObject problem = (
            await response.Content.ReadFromJsonAsync<JsonObject>(Ct)
        ).ShouldNotBeNull();
        problem["code"]!.GetValue<string>().ShouldBe("record_changed");
    }

    private Task<HttpResponseMessage> PostSubmit(string revisionHash) =>
        _httpClient.PostAsJsonAsync(
            new Uri($"/records/{_record.Id}/submit", UriKind.Relative),
            new { formVersion = FormVersions.Medicine, revisionHash },
            Ct
        );

    /// <summary>The hash check your answers would give for the revision as it is now.</summary>
    private async Task<string> CurrentHash()
    {
        ServiceTestHarness<RecordFormPageLoader> harness = new(Context);
        harness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = _user.CognitoUsername,
                OrganisationId = _organisation.Id,
                UserRole = UserRole.Standard,
            }
        );
        RecordFormPageLoader loader = harness.Service;
        var (form, _) = await loader.LoadFormAsync(_record.Id, track: false, Ct);
        var (answers, _) = await loader.LoadAllAnswersAsync(form!, Ct);
        return AnswerHash.ForRevision(form!.Revision.Id, answers);
    }
}
