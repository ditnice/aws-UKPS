using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;
using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.SaveRecordPageError
>;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// The MHRA procedure and dates section against a real database: date questions stored as
/// regulatory dates, and conditions on a reference data code and on a date's precision.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RecordFormMhraServiceTests : DatabaseTestBase
{
    private const string ProcedureType = "record_mhra_procedure.mhra_procedure_type_id";
    private const string ProcedureDetails = "record_mhra_procedure.procedure_details";
    private const string Regulator = "record_mhra_procedure.irp_reference_regulator_id";
    private const string Route = "medicines_intl_recognition.irp_route_id";
    private const string UkSubmission = "record_mhra_date.uk_submission_date";
    private const string UkLicence = "record_mhra_date.uk_licence_date";

    private const string QuarterAudit =
        """{"date_precision":"EstimatedQuarter","date_value":"2027-04-01","is_confidential":true}""";
    private const string ActualAudit =
        """{"date_precision":"ActualDate","date_value":"2026-05-14","is_confidential":false}""";
    private const string GlobalRegion =
        "medicines_global_submission.global_first_submission_region";
    private const string GlobalDate = "medicines_global_submission.global_submission_actual_date";
    private const string IntlSubmission = "medicines_intl_recognition.intl_submission_date";

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;
    private MhraProcedureType _national = null!;
    private MhraProcedureType _irp = null!;
    private IrpReferenceRegulator _regulator = null!;
    private IrpRoute _route = null!;
    private ServiceTestHarness<IRecordFormQueryService> _queryHarness = null!;
    private ServiceTestHarness<IRecordFormSaveService> _saveHarness = null!;

    public RecordFormMhraServiceTests(PostgresFixture fixture)
        : base(fixture) { }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(new OrganisationFaker().Generate(), Ct);
        _user = await AddEntity(new UserFaker().Generate(), Ct);
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

        _national = await AddEntity(
            new MhraProcedureType
            {
                Code = MhraProcedureTypeCodes.National,
                Label = "MHRA national procedure",
                RelevantTo = ReferenceDataType.Shared,
            },
            Ct
        );
        _irp = await AddEntity(
            new MhraProcedureType
            {
                Code = MhraProcedureTypeCodes.InternationalRecognition,
                Label = "International Recognition Procedure",
                RelevantTo = ReferenceDataType.Shared,
            },
            Ct
        );
        await AddEntity(
            new MhraProcedureType
            {
                Code = "vaccines-only",
                Label = "A vaccines-only procedure",
                RelevantTo = ReferenceDataType.VaccinesOnly,
            },
            Ct
        );
        _regulator = await AddEntity(new IrpReferenceRegulator { Label = "Australia" }, Ct);
        _route = await AddEntity(new IrpRoute { Label = "Route A" }, Ct);

        _queryHarness = CreateHarness<IRecordFormQueryService>();
        _saveHarness = CreateHarness<IRecordFormSaveService>();
    }

    [Fact]
    public async Task GetPage_UkMarketAuthorisation_HasTheProcedureAndItsDetails()
    {
        RecordPageDto page = await GetPage("uk-market-authorisation");

        page.Page.CommercialInConfidence.ShouldBeTrue();
        page.Questions.Select(static q => q.Id).ShouldBe([ProcedureType, ProcedureDetails]);
        page.Questions[0]
            .Options!.Select(static o => o.Label)
            .ShouldBe(["MHRA national procedure", "International Recognition Procedure"]);
    }

    [Fact]
    public async Task SavePage_Irp_ShowsTheIrpPages()
    {
        (await GetPage("uk-conditional-approval")).NextPageId.ShouldBeNull();
        (await _queryHarness.Service.GetPage(_record.Id, "international-recognition", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();

        await SaveOk("uk-market-authorisation", Procedure(_irp));
        await SaveOk("international-recognition", Irp(Id(_regulator), Id(_route)));

        (
            await Context.RecordMhraProcedures.AsNoTracking().SingleAsync(Ct)
        ).IrpReferenceRegulatorId.ShouldBe(_regulator.Id);
        (
            await Context.MedicinesIntlRecognitions.AsNoTracking().SingleAsync(Ct)
        ).IrpRouteId.ShouldBe(_route.Id);
        (await GetPage("uk-conditional-approval")).NextPageId.ShouldBe("international-recognition");
        RecordPageDto irp = await GetPage("international-recognition");
        irp.Questions.Select(static q => q.Id).ShouldBe([Regulator, Route]);
        irp.NextPageId.ShouldBe("international-submission-date");
    }

    [Fact]
    public async Task SavePage_IrpPage_RequiresTheRegulatorAndRoute()
    {
        await SaveOk("uk-market-authorisation", Procedure(_irp));

        (await SaveInvalid("international-recognition", Irp(null, null))).Keys.ShouldBe(
            [Regulator, Route],
            ignoreOrder: true
        );
    }

    [Fact]
    public async Task SavePage_ProcedureNoLongerIrp_ClearsTheIrpAnswersAndHidesTheIrpPages()
    {
        await SaveOk("uk-market-authorisation", Procedure(_irp));
        await SaveOk("international-recognition", Irp(Id(_regulator), Id(_route)));
        await SaveOk("international-submission-date", Answer(IntlSubmission, "2027-Q1"));

        await SaveOk("uk-market-authorisation", Procedure(_national));

        (
            await Context.RecordMhraProcedures.AsNoTracking().SingleAsync(Ct)
        ).IrpReferenceRegulatorId.ShouldBeNull();
        MedicinesIntlRecognition intl = await Context
            .MedicinesIntlRecognitions.AsNoTracking()
            .SingleAsync(Ct);
        intl.IrpRouteId.ShouldBeNull();
        intl.IntlSubmissionDateId.ShouldBeNull();
        (await Context.RegulatoryDates.AnyAsync(Ct)).ShouldBeFalse();
        (
            await Context
                .RecordFieldAudits.AsNoTracking()
                .SingleAsync(a => a.FieldPath == IntlSubmission && a.NewValue == null, Ct)
        ).OldValue.ShouldBe(
            """{"date_precision":"EstimatedQuarter","date_value":"2027-01-01","is_confidential":true}"""
        );
        (await _queryHarness.Service.GetPage(_record.Id, "international-recognition", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();
    }

    [Fact]
    public async Task SavePage_PrecisionDate_StoresAndUpdatesOneRegulatoryDate()
    {
        await SaveOk("uk-licence-date", Answer(UkLicence, "2027-Q2"));

        RegulatoryDate quarter = await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct);
        (quarter.RevisionId, quarter.DateEvent, quarter.DatePrecision, quarter.DateValue).ShouldBe(
            (
                _revision.Id,
                DateEventType.UkLicence,
                DatePrecision.EstimatedQuarter,
                new DateOnly(2027, 4, 1)
            )
        );
        (await Context.RecordMhraDates.AsNoTracking().SingleAsync(Ct)).UkLicenceDateId.ShouldBe(
            quarter.Id
        );

        await SaveOk("uk-licence-date", Answer(UkLicence, "2026-05-14"));

        RegulatoryDate actual = await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct);
        (actual.Id, actual.DatePrecision, actual.DateValue).ShouldBe(
            (quarter.Id, DatePrecision.ActualDate, new DateOnly(2026, 5, 14))
        );
        (await GetPage("uk-licence-date")).Answers[UkLicence].GetString().ShouldBe("2026-05-14");
    }

    [Theory]
    [InlineData("2027-Q2", true)]
    [InlineData("2027-05", true)]
    [InlineData("2026-05-14", false)]
    public async Task SavePage_PrecisionDate_IsConfidentialOnlyWhileEstimated(
        string answer,
        bool confidential
    )
    {
        await SaveOk("uk-submission-date", Answer(UkSubmission, answer));

        (await Context.RegulatoryDates.AsNoTracking().SingleAsync(Ct)).IsConfidential.ShouldBe(
            confidential
        );
    }

    [Fact]
    public async Task SavePage_PrecisionDate_AuditsTheRegulatoryDateAsJson()
    {
        await SaveOk("uk-licence-date", Answer(UkLicence, "2027-Q2"));
        await SaveOk("uk-licence-date", Answer(UkLicence, "2026-05-14"));

        (
            await Context
                .RecordFieldAudits.AsNoTracking()
                .Where(static a => a.FieldPath == UkLicence)
                .OrderBy(static a => a.Id)
                .Select(static a => new { a.OldValue, a.NewValue })
                .ToListAsync(Ct)
        ).ShouldBe([
            new { OldValue = (string?)null, NewValue = (string?)QuarterAudit },
            new { OldValue = (string?)QuarterAudit, NewValue = (string?)ActualAudit },
        ]);
    }

    [Theory]
    [InlineData("uk-submission-date", UkSubmission, "2027-02-29", true)]
    [InlineData("uk-submission-date", UkSubmission, "2027-13", false)]
    [InlineData("uk-submission-date", UkSubmission, "2027-Q5", false)]
    [InlineData("uk-submission-date", UkSubmission, "27-01-01", true)]
    [InlineData("uk-submission-date", UkSubmission, "next spring", false)]
    [InlineData("international-submission-date", IntlSubmission, "2027-1", false)]
    public async Task SavePage_DateThatIsNotReal_IsRejected(
        string pageId,
        string questionId,
        string answer,
        bool actual
    )
    {
        if (string.Equals(pageId, "international-submission-date", StringComparison.Ordinal))
        {
            await SaveOk("uk-market-authorisation", Procedure(_irp));
        }

        (await SaveInvalid(pageId, Answer(questionId, answer))).ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [questionId] = RecordFormMessages.DateNotReal(actual, "submission"),
            }
        );
    }

    [Fact]
    public async Task SavePage_ActualUkSubmissionDate_ShowsTheGlobalFirstSubmissionPage()
    {
        (
            await SaveOk("uk-submission-date", Answer(UkSubmission, "2026-01-05"))
        ).NextPageId.ShouldBe("global-first-submission");

        await SaveOk("global-first-submission", Global("Australia", "2025-07"));

        (
            await Context.MedicinesGlobalSubmissions.AsNoTracking().SingleAsync(Ct)
        ).GlobalFirstSubmissionRegion.ShouldBe("Australia");
        RegulatoryDate date = await Context
            .RegulatoryDates.AsNoTracking()
            .SingleAsync(static d => d.DateEvent == DateEventType.GlobalFirstSubmission, Ct);
        (date.DatePrecision, date.DateValue, date.IsConfidential).ShouldBe(
            (DatePrecision.ActualMonth, new DateOnly(2025, 7, 1), false)
        );
        (await GetPage("global-first-submission"))
            .Answers[GlobalDate]
            .GetString()
            .ShouldBe("2025-07");
    }

    [Theory]
    [InlineData("2026-11", RecordFormMessages.ActualMonthInFuture)]
    [InlineData("2025-07-26", RecordFormMessages.InvalidDate)]
    [InlineData("2025-Q3", RecordFormMessages.InvalidDate)]
    public async Task SavePage_GlobalFirstSubmission_TakesAMonthNotAfterThisOne(
        string answer,
        string error
    )
    {
        await SaveOk("uk-submission-date", Answer(UkSubmission, "2026-01-05"));

        (await SaveInvalid("global-first-submission", Global("Australia", answer))).ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal) { [GlobalDate] = error }
        );
    }

    [Fact]
    public async Task SavePage_EstimatedUkSubmissionDate_ClearsTheGlobalFirstSubmission()
    {
        await SaveOk("uk-submission-date", Answer(UkSubmission, "2026-01-05"));
        await SaveOk("global-first-submission", Global("Australia", "2025-07"));

        (await SaveOk("uk-submission-date", Answer(UkSubmission, "2027-03"))).NextPageId.ShouldBe(
            "uk-licence-date"
        );

        MedicinesGlobalSubmission global = await Context
            .MedicinesGlobalSubmissions.AsNoTracking()
            .SingleAsync(Ct);
        global.GlobalFirstSubmissionRegion.ShouldBeNull();
        global.GlobalSubmissionActualDateId.ShouldBeNull();
        (
            await Context
                .RegulatoryDates.AsNoTracking()
                .Select(static d => d.DateEvent)
                .ToListAsync(Ct)
        ).ShouldBe([DateEventType.UkSubmission]);
        (await _queryHarness.Service.GetPage(_record.Id, "global-first-submission", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();
    }

    private static string Id(MhraProcedureType item) =>
        item.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Id(IrpReferenceRegulator item) =>
        item.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Id(IrpRoute item) =>
        item.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static Dictionary<string, JsonElement> Procedure(MhraProcedureType type) =>
        new(StringComparer.Ordinal)
        {
            [ProcedureType] = Json(Id(type)),
            [ProcedureDetails] = Json<string?>(null),
        };

    private static Dictionary<string, JsonElement> Irp(string? regulator, string? route) =>
        new(StringComparer.Ordinal) { [Regulator] = Json(regulator), [Route] = Json(route) };

    private static Dictionary<string, JsonElement> Global(string region, string date) =>
        new(StringComparer.Ordinal) { [GlobalRegion] = Json(region), [GlobalDate] = Json(date) };

    private ServiceTestHarness<TService> CreateHarness<TService>()
        where TService : notnull
    {
        ServiceTestHarness<TService> harness = new(Context);
        harness
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = _organisation.Id,
                    UserRole = UserRole.Standard,
                }
            )
            .UpdateCurrentTime(_now);
        return harness;
    }

    private async Task<RecordPageDto> GetPage(string pageId) =>
        (await _queryHarness.Service.GetPage(_record.Id, pageId, Ct)).ShouldBeSuccess();

    private async Task<SaveRecordPageResult> Save(
        string pageId,
        Dictionary<string, JsonElement> answers
    )
    {
        RecordPageDto page = await GetPage(pageId);
        return await _saveHarness.Service.SavePage(
            _record.Id,
            pageId,
            new SaveRecordPageCommand
            {
                FormVersion = FormVersions.Medicine,
                Answers = answers,
                PageHash = page.PageHash,
            },
            Ct
        );
    }

    private async Task<SaveRecordPageDto> SaveOk(
        string pageId,
        Dictionary<string, JsonElement> answers
    ) => (await Save(pageId, answers)).ShouldBeSuccess();

    private async Task<IReadOnlyDictionary<string, string>> SaveInvalid(
        string pageId,
        Dictionary<string, JsonElement> answers
    ) =>
        (await Save(pageId, answers))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors;

    private static JsonElement Json<T>(T value) => JsonSerializer.SerializeToElement(value);

    private static Dictionary<string, JsonElement> Answer<T>(string questionId, T value) =>
        new(StringComparer.Ordinal) { [questionId] = Json(value) };
}
