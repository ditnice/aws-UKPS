using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.InternalServices.Identity;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class RecordStatusEndpointTests : DatabaseTestBase
{
    private readonly HttpClient _httpClient;
    private Organisation _organisation = null!;
    private User _user = null!;

    public RecordStatusEndpointTests(PostgresFixture fixture)
        : base(fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _httpClient = fixture.Factory.CreateClient();
        Randomizer.Seed = new Random(219);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var cancellationToken = TestContext.Current.CancellationToken;
        _organisation = await AddEntity(new OrganisationFaker().Generate(), cancellationToken);

        // The caller's Cognito username, so the change is attributed to this user.
        string username = TestAuthenticationOptions
            .DefaultClaims.Single(c =>
                string.Equals(c.Type, UkpsClaimTypes.Username, StringComparison.Ordinal)
            )
            .Value;
        _user = await AddEntity(
            new UserFaker()
                .RuleFor(x => x.CognitoUsername, CognitoUsername.Parse(username))
                .Generate(),
            cancellationToken
        );
        AuthenticateAs(UserRole.Standard, _organisation.Id);
    }

    [Fact]
    public async Task PutOnHold_ThenGetRecord_ReturnsNewStatusAndActions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddPublishedRecord();

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "hold",
            new { reason = "TrialSuspended", note = "ION-UK paused pending safety review" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonObject dto = (
            await _httpClient.GetFromJsonAsync<JsonObject>(
                new Uri(
                    $"/records/{record.Id}?recordType=Medicine&organisationId={_organisation.Id}",
                    UriKind.Relative
                ),
                cancellationToken
            )
        ).ShouldNotBeNull();
        dto["recordStatus"]!.GetValue<string>().ShouldBe(nameof(RecordStatus.OnHold));
        dto["displayStatus"]!.GetValue<string>().ShouldBe(nameof(RecordStatus.OnHold));
        dto["actions"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ShouldBe([
                nameof(RecordAction.ChangeStatus),
                nameof(RecordAction.ConfirmOnHold),
                nameof(RecordAction.ViewTimeline),
            ]);

        RecordStatusHistory history = await Context
            .RecordStatusHistories.AsNoTracking()
            .SingleAsync(h => h.RecordId == record.Id, cancellationToken);
        history.Reason.ShouldBe(RecordStatusChangeReason.TrialSuspended);
        history.Note.ShouldBe("ION-UK paused pending safety review");
        history.UpdatedBy.ShouldBe(_user.Id);
    }

    [Fact]
    public async Task Resume_WhenRecordIsPublished_ReturnsConflictProblem()
    {
        Record record = await AddPublishedRecord();

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "resume",
            new { reason = "TrialResumed" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        JsonObject problem = (
            await response.Content.ReadFromJsonAsync<JsonObject>(
                TestContext.Current.CancellationToken
            )
        ).ShouldNotBeNull();
        problem["status"]!.GetValue<int>().ShouldBe(StatusCodes.Status409Conflict);
        problem["code"]!.GetValue<string>().ShouldBe("status_change_not_permitted");
    }

    [Theory]
    [InlineData("hold", "TrialResumed")]
    [InlineData("resume", "TrialSuspended")]
    [InlineData("archive", "ArchivedAutomatically")]
    [InlineData("archive", "ManuallyDiscarded")]
    public async Task ChangeStatus_WhenReasonIsNotForTheChange_ReturnsValidationProblem(
        string endpoint,
        string reason
    )
    {
        Record record = await AddPublishedRecord();

        using HttpResponseMessage response = await PatchStatus(record.Id, endpoint, new { reason });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
                TestContext.Current.CancellationToken
            );
        problem.ShouldNotBeNull().Errors.Keys.ShouldBe(["Reason"]);
    }

    [Fact]
    public async Task Archive_WithOtherAndANote_SavesTheNote()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddPublishedRecord();

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "archive",
            new { reason = "Other", note = "Rights sold to another company" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RecordStatusHistory history = await Context
            .RecordStatusHistories.AsNoTracking()
            .SingleAsync(h => h.RecordId == record.Id, cancellationToken);
        history.Reason.ShouldBe(RecordStatusChangeReason.Other);
        history.Note.ShouldBe("Rights sold to another company");
    }

    [Fact]
    public async Task Archive_WithOtherAndNoNote_ReturnsValidationProblem()
    {
        Record record = await AddPublishedRecord();

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "archive",
            new { reason = "Other" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
                TestContext.Current.CancellationToken
            );
        problem.ShouldNotBeNull().Errors.Keys.ShouldBe(["Note"]);
    }

    [Fact]
    public async Task PutOnHold_WhenCallerIsSuperUser_ReturnsForbiddenAndGetRecordOmitsChangeStatus()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddPublishedRecord();
        AuthenticateAs(UserRole.Super, _organisation.Id);

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "hold",
            new { reason = "TrialSuspended" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        JsonObject dto = (
            await _httpClient.GetFromJsonAsync<JsonObject>(
                new Uri(
                    $"/records/{record.Id}?recordType=Medicine&organisationId={_organisation.Id}",
                    UriKind.Relative
                ),
                cancellationToken
            )
        ).ShouldNotBeNull();
        dto["recordStatus"]!.GetValue<string>().ShouldBe(nameof(RecordStatus.Published));
        dto["actions"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ShouldNotContain(nameof(RecordAction.ChangeStatus));
    }

    [Fact]
    public async Task PutOnHold_RecordInAnotherOrganisationOrMissing_ReturnsIndistinguishableNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            cancellationToken
        );
        Record otherRecord = await AddPublishedRecord(otherOrganisation.Id);

        using HttpResponseMessage guessedIdResponse = await PatchStatus(
            otherRecord.Id,
            "hold",
            new { reason = "TrialSuspended" }
        );
        using HttpResponseMessage otherOrganisationResponse = await PatchStatus(
            otherRecord.Id,
            "hold",
            new { reason = "TrialSuspended" },
            otherOrganisation.Id
        );
        using HttpResponseMessage missingResponse = await PatchStatus(
            999999,
            "hold",
            new { reason = "TrialSuspended" }
        );

        JsonObject[] bodies = await Task.WhenAll(
            new[] { guessedIdResponse, otherOrganisationResponse, missingResponse }.Select(
                async r =>
                {
                    r.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                    JsonObject body = (
                        await r.Content.ReadFromJsonAsync<JsonObject>(cancellationToken)
                    ).ShouldNotBeNull();
                    body.Remove("traceId");
                    return body;
                }
            )
        );
        JsonNode.DeepEquals(bodies[0], bodies[1]).ShouldBeTrue();
        JsonNode.DeepEquals(bodies[0], bodies[2]).ShouldBeTrue();
        (
            await Context
                .Records.AsNoTracking()
                .SingleAsync(r => r.Id == otherRecord.Id, cancellationToken)
        ).RecordStatus.ShouldBe(RecordStatus.Published);
    }

    [Fact]
    public async Task PutOnHold_WhenOrganisationIsSelectableButNotSelected_ReturnsMismatchConflict()
    {
        Organisation selectedOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        Record record = await AddPublishedRecord();
        AuthenticateAs(UserRole.Standard, selectedOrganisation.Id);
        Fixture.Factory.AuthOptions.Claims.Add(
            new Claim(UkpsClaimTypes.SelectableOrganisationId, $"{_organisation.Id}")
        );

        using HttpResponseMessage response = await PatchStatus(
            record.Id,
            "hold",
            new { reason = "TrialSuspended" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        JsonObject problem = (
            await response.Content.ReadFromJsonAsync<JsonObject>(
                TestContext.Current.CancellationToken
            )
        ).ShouldNotBeNull();
        problem["code"]!.GetValue<string>().ShouldBe("SelectedOrganisationMismatch");
        problem["organisationId"]!.GetValue<int>().ShouldBe(_organisation.Id);
    }

    private Task<HttpResponseMessage> PatchStatus(
        int recordId,
        string endpoint,
        object body,
        int? organisationId = null
    ) =>
        _httpClient.PatchAsJsonAsync(
            new Uri(
                $"/records/{recordId}/{endpoint}?organisationId={organisationId ?? _organisation.Id}",
                UriKind.Relative
            ),
            body,
            TestContext.Current.CancellationToken
        );

    private async Task<Record> AddPublishedRecord(int? organisationId = null)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, organisationId ?? _organisation.Id)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, RecordStatus.Published)
                .Generate(),
            cancellationToken
        );
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Published)
                .Generate(),
            cancellationToken
        );
        Context.ChangeTracker.Clear();
        return record;
    }
}
