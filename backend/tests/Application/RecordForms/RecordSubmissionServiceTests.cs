using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;
using SubmitRecordResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.RecordForms.Errors.SubmitRecordError>;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>The submit service against a real database, starting from a complete draft.</summary>
[Collection(DatabaseCollection.Name)]
public class RecordSubmissionServiceTests : DatabaseTestBase
{
    private const string IndicationIsCancer = "medicines_indication_detail.indication_is_cancer";
    private const string ScreeningDetails = "medicines_patient_identification.screening_details";
    private const string Indication = "medicines_indication_detail.indication";
    private const string UkLaunchDate = "record_hta.uk_launch_date";

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateOnly _launchQuarter = new(2027, 4, 1);

    private static readonly FormModel _model = FormModelFactory
        .Create(OptionProviderRegistry.CreateDefault())
        .Find(RecordType.Medicine)!;

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;

    public RecordSubmissionServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(585);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(new OrganisationFaker().Generate(), Ct);
        _user = await AddEntity(new UserFaker().Generate(), Ct);
        _record = await AddRecord(_organisation.Id, RecordStatus.Unpublished);
        _revision = await AddRevision(_record, WorkflowStatus.Draft);
        await CompleteMedicineRevision.AddAsync(Context, _revision.Id, _launchQuarter, Ct);
    }

    [Fact]
    public async Task Submit_CompleteDraft_GoesToQaReview()
    {
        DateTime? reviewedAt = await Context
            .Records.AsNoTracking()
            .Where(r => r.Id == _record.Id)
            .Select(static r => r.ReviewedAt)
            .SingleAsync(Ct);

        (await Submit(await CurrentHash())).ShouldBeSuccess();

        RecordRevision revision = await Context
            .RecordRevisions.AsNoTracking()
            .Include(static r => r.QaReviews)
            .SingleAsync(r => r.Id == _revision.Id, Ct);
        revision.WorkflowStatus.ShouldBe(WorkflowStatus.QAReview);
        QaReview review = revision.QaReviews.ShouldHaveSingleItem();
        review.Outcome.ShouldBeNull();
        review.ReviewedBy.ShouldBeNull();
        review.ReviewedAt.ShouldBeNull();

        RecordEvent recordEvent = await Context
            .RecordEvents.AsNoTracking()
            .SingleAsync(e => e.RecordId == _record.Id, Ct);
        recordEvent.EventType.ShouldBe(RecordEventType.SubmittedToQa);
        recordEvent.RevisionId.ShouldBe(_revision.Id);
        recordEvent.QaReviewId.ShouldBe(review.Id);
        recordEvent.PerformedBy.ShouldBe(_user.Id);
        recordEvent.PerformedAt.ShouldBe(_now);

        Record record = await Context
            .Records.AsNoTracking()
            .SingleAsync(r => r.Id == _record.Id, Ct);
        record.RecordStatus.ShouldBe(RecordStatus.Unpublished);
        record.ReviewedAt.ShouldBe(reviewedAt);
        (
            await Context.RecordStatusHistories.AnyAsync(h => h.RecordId == _record.Id, Ct)
        ).ShouldBeFalse();
    }

    [Fact]
    public async Task Submit_DraftOfAPublishedRecord_StaysPublished()
    {
        await SetRecordStatus(RecordStatus.Published);

        (await Submit(await CurrentHash())).ShouldBeSuccess();

        (
            await Context.Records.AsNoTracking().SingleAsync(r => r.Id == _record.Id, Ct)
        ).RecordStatus.ShouldBe(RecordStatus.Published);
        (await RevisionStatus()).ShouldBe(WorkflowStatus.QAReview);
    }

    [Fact]
    public async Task Submit_AnswerChangedSinceTheHashWasTaken_IsRecordChanged()
    {
        string hash = await CurrentHash();
        await UpdateRow<MedicinesTreatmentDetail>(x => x.ProposedPlaceInTherapy = "Second line");

        (await Submit(hash)).ShouldBeError().ShouldBeOfType<SubmitRecordError.RecordChanged>();
        (await RevisionStatus()).ShouldBe(WorkflowStatus.Draft);
    }

    [Fact]
    public async Task Submit_PageSavedWhileSubmitting_IsRecordChangedAndWritesNothing()
    {
        string hash = await CurrentHash();
        await using AppDbContext context = Fixture.CreateContext(
            new SaveRevisionFirstInterceptor(_revision.Id)
        );

        (await Harness(context: context).Service.Submit(_record.Id, Command(hash), Ct))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.RecordChanged>();
        (await RevisionStatus()).ShouldBe(WorkflowStatus.Draft);
        (await Context.QaReviews.AnyAsync(Ct)).ShouldBeFalse();
        (await Context.RecordEvents.AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Submit_HashOfAnotherRevisionWithTheSameAnswers_IsRecordChanged()
    {
        var (answers, _) = await LoadAnswers();

        (await Submit(AnswerHash.ForRevision(_revision.Id + 1, answers)))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.RecordChanged>();
    }

    [Fact]
    public async Task Submit_StaleHashAndInvalidAnswers_ReportsTheChangeFirst()
    {
        string hash = await CurrentHash();
        await UpdateRow<MedicinesIndicationDetail>(x => x.IndicationIsCancer = null);

        (await Submit(hash)).ShouldBeError().ShouldBeOfType<SubmitRecordError.RecordChanged>();
    }

    [Fact]
    public async Task Submit_FormVersionChanged_IsRejected()
    {
        (await Submit(await CurrentHash(), formVersion: "2000.1.1"))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.FormVersionChanged>();
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.QAReview)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Published)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Draft)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.Draft)]
    public async Task Submit_NotASubmittableDraft_IsNotSubmittable(
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus
    )
    {
        await SetRecordStatus(recordStatus);
        await Context
            .RecordRevisions.Where(r => r.Id == _revision.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.WorkflowStatus, revisionStatus), Ct);

        (await Submit(await CurrentHash()))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.NotSubmittable>();
    }

    [Fact]
    public async Task Submit_UnknownRecord_IsNotFound()
    {
        (await Harness().Service.Submit(_record.Id + 1000, Command("hash"), Ct))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.NotFound>();
    }

    [Fact]
    public async Task Submit_AnotherOrganisationsRecord_IsNotAllowed()
    {
        Organisation other = await AddEntity(new OrganisationFaker().Generate(), Ct);

        (await Harness(organisationId: other.Id).Service.Submit(_record.Id, Command("hash"), Ct))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.NotAllowed>();
    }

    [Fact]
    public async Task Submit_SuperUser_CannotSubmit()
    {
        string hash = await CurrentHash();

        (await Harness(UserRole.Super).Service.Submit(_record.Id, Command(hash), Ct))
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.CannotSubmit>();
    }

    [Fact]
    public async Task Submit_ChampionUser_CanSubmit()
    {
        string hash = await CurrentHash();

        (
            await Harness(UserRole.Champion).Service.Submit(_record.Id, Command(hash), Ct)
        ).ShouldBeSuccess();
    }

    [Fact]
    public async Task Submit_RequiredAnswersMissing_ReportsEachInFormOrderAndWritesNothing()
    {
        // A required question left unanswered, and a required follow-up revealed but unanswered.
        await UpdateRow<MedicinesIndicationDetail>(x => x.IndicationIsCancer = null);
        await UpdateRow<MedicinesPatientIdentification>(x =>
            x.ScreeningRequired = YesNoUnknown.Yes
        );

        IReadOnlyList<SubmissionErrorDto> errors = await SubmitInvalid();

        errors.ShouldBe([
            Error(IndicationIsCancer, Message(IndicationIsCancer)),
            Error(ScreeningDetails, Message(ScreeningDetails)),
        ]);
        (await RevisionStatus()).ShouldBe(WorkflowStatus.Draft);
        (await Context.QaReviews.AnyAsync(Ct)).ShouldBeFalse();
        (await Context.RecordEvents.AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Submit_HiddenRequiredQuestionsUnanswered_AreNotReported()
    {
        // Screening details only applies when screening is required, which it isn't.
        (await Submit(await CurrentHash())).ShouldBeSuccess();
    }

    [Fact]
    public async Task Submit_EstimatedDateNowInThePast_IsReported()
    {
        await Context
            .RegulatoryDates.Where(d => d.RevisionId == _revision.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.DateValue, new DateOnly(2026, 4, 1)), Ct);

        IReadOnlyList<SubmissionErrorDto> errors = await SubmitInvalid();

        errors.ShouldBe([Error(UkLaunchDate, RecordFormMessages.EstimatedDateInPast)]);
    }

    [Fact]
    public async Task Submit_TextOverItsMaximumLength_IsReported()
    {
        await UpdateRow<MedicinesIndicationDetail>(x => x.Indication = new string('a', 5000));

        IReadOnlyList<SubmissionErrorDto> errors = await SubmitInvalid();

        errors.ShouldHaveSingleItem().QuestionId.ShouldBe(Indication);
    }

    [Fact]
    public async Task Submit_CrossPageRuleFails_ReportsItOnEveryQuestionItNames()
    {
        CrossPageRule rule = new FailingRule("TEST-01", Indication, UkLaunchDate);

        IReadOnlyList<SubmissionErrorDto> errors = await SubmitInvalid(rule);

        errors.ShouldBe([
            Error(Indication, $"TEST-01 {Indication}"),
            Error(UkLaunchDate, $"TEST-01 {UkLaunchDate}"),
        ]);
    }

    [Fact]
    public async Task Submit_CrossPageRuleOnAQuestionWithAPageError_IsSkipped()
    {
        await UpdateRow<MedicinesIndicationDetail>(x => x.IndicationIsCancer = null);
        CrossPageRule rule = new FailingRule("TEST-01", IndicationIsCancer, UkLaunchDate);

        IReadOnlyList<SubmissionErrorDto> errors = await SubmitInvalid(rule);

        errors.ShouldBe([Error(IndicationIsCancer, Message(IndicationIsCancer))]);
    }

    private async Task<IReadOnlyList<SubmissionErrorDto>> SubmitInvalid(
        params CrossPageRule[] rules
    )
    {
        string hash = await CurrentHash();
        ServiceTestHarness<IRecordSubmissionService> harness = Harness();
        if (rules.Length > 0)
        {
            harness.ConfigureServices(services =>
                services.AddSingleton(sp =>
                    CrossPageRuleRegistry.Create(
                        sp.GetRequiredService<FormModelRegistry>(),
                        new Dictionary<RecordType, IReadOnlyList<CrossPageRule>>
                        {
                            [RecordType.Medicine] = rules,
                        }
                    )
                )
            );
        }

        SubmitRecordError error = (
            await harness.Service.Submit(_record.Id, Command(hash), Ct)
        ).ShouldBeError();
        return error.ShouldBeOfType<SubmitRecordError.Invalid>().Errors;
    }

    private static SubmissionErrorDto Error(string questionId, string message)
    {
        FormPage page = _model.PageOf(_model.FindQuestion(questionId)!);
        return new SubmissionErrorDto
        {
            QuestionId = questionId,
            PageId = page.Id,
            TaskId = page.TaskId,
            Message = message,
        };
    }

    private static string Message(string questionId) =>
        _model
            .FindQuestion(questionId)!
            .Rules.Single(static r => r.Kind == RuleKind.Required)
            .Message;

    private async Task<SubmitRecordResult> Submit(
        string revisionHash,
        string formVersion = FormVersions.Medicine
    ) => await Harness().Service.Submit(_record.Id, Command(revisionHash, formVersion), Ct);

    private static SubmitRecordCommand Command(
        string revisionHash,
        string formVersion = FormVersions.Medicine
    ) => new() { FormVersion = formVersion, RevisionHash = revisionHash };

    /// <summary>The hash check your answers would give for the revision as it is now.</summary>
    private async Task<string> CurrentHash()
    {
        var (answers, revisionId) = await LoadAnswers();
        return AnswerHash.ForRevision(revisionId, answers);
    }

    private async Task<(Dictionary<string, FormAnswer> Answers, int RevisionId)> LoadAnswers()
    {
        RecordFormPageLoader loader = Harness<RecordFormPageLoader>().Service;
        var (form, _) = await loader.LoadFormAsync(_record.Id, track: false, Ct);
        var (answers, _) = await loader.LoadAllAnswersAsync(form!, Ct);
        return (answers, form!.Revision.Id);
    }

    private ServiceTestHarness<IRecordSubmissionService> Harness(
        UserRole role = UserRole.Standard,
        int? organisationId = null,
        AppDbContext? context = null
    ) => Harness<IRecordSubmissionService>(role, organisationId, context);

    private ServiceTestHarness<TService> Harness<TService>(
        UserRole role = UserRole.Standard,
        int? organisationId = null,
        AppDbContext? context = null
    )
        where TService : notnull
    {
        ServiceTestHarness<TService> harness = new(context ?? Context);
        harness
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = organisationId ?? _organisation.Id,
                    UserRole = role,
                }
            )
            .UpdateCurrentTime(_now);
        return harness;
    }

    private async Task<Record> AddRecord(int organisationId, RecordStatus status) =>
        await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, organisationId)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, status)
                .Generate(),
            Ct
        );

    private async Task<RecordRevision> AddRevision(Record record, WorkflowStatus status) =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, status)
                .Generate(),
            Ct
        );

    private async Task SetRecordStatus(RecordStatus status) =>
        await Context
            .Records.Where(r => r.Id == _record.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.RecordStatus, status), Ct);

    private async Task<WorkflowStatus> RevisionStatus() =>
        await Context
            .RecordRevisions.AsNoTracking()
            .Where(r => r.Id == _revision.Id)
            .Select(static r => r.WorkflowStatus)
            .SingleAsync(Ct);

    private async Task UpdateRow<TEntity>(Action<TEntity> update)
        where TEntity : class
    {
        Context.ChangeTracker.Clear();
        TEntity row = await Context
            .Set<TEntity>()
            .SingleAsync(x => EF.Property<int>(x, "RevisionId") == _revision.Id, Ct);
        update(row);
        await Context.SaveChangesAsync(Ct);
        Context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Touches the revision row just before a save writes, as a page save landing mid-submit
    /// would, so the revision's concurrency token has moved on.
    /// </summary>
    private sealed class SaveRevisionFirstInterceptor(int revisionId) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
        )
        {
            await eventData
                .Context!.Set<RecordRevision>()
                .Where(r => r.Id == revisionId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(r => r.WorkflowStatus, r => r.WorkflowStatus),
                    cancellationToken
                );
            return result;
        }
    }

    /// <summary>Fails on every question it names, with a message naming the rule and question.</summary>
    private sealed record FailingRule(string Id, params string[] Questions) : CrossPageRule(Id)
    {
        public override IReadOnlyList<string> QuestionIds => Questions;

        public override IEnumerable<(string QuestionId, string Message)> Check(
            FormModel model,
            IReadOnlyDictionary<string, FormAnswer> answers,
            Applicability applicability
        ) => Questions.Select(q => (q, $"{Id} {q}"));
    }
}
