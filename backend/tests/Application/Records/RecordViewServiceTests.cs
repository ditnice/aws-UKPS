using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetRecordDetailResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.RecordDetailDto,
    UKPS.Api.Application.Records.Errors.GetRecordError
>;
using GetRecordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.RecordOverviewDto,
    UKPS.Api.Application.Records.Errors.GetRecordError
>;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordViewServiceTests : DatabaseTestBase
{
    private const string Indication = "medicines_indication_detail.indication";
    private const string IndicationAnswer = "Chronic hepatitis C virus infection in adults";
    private const string IndicationIsCancer = "medicines_indication_detail.indication_is_cancer";
    private const string ScreeningDetails = "medicines_patient_identification.screening_details";
    private const string ClinicalTrials = "record_clinical_trial";

    private static readonly DateTime _reviewedAt = new(2026, 6, 19, 12, 0, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private IServiceTestHarness<IRecordViewService> _harness = null!;
    private IRecordViewService Service => _harness.Service;

    public RecordViewServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(547);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        _user = await AddEntity(new UserFaker().Generate(), TestContext.Current.CancellationToken);

        _harness = new ServiceTestHarness<IRecordViewService>(Context).UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = _organisation.Id,
                UserRole = UserRole.Standard,
            }
        );
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.Draft)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Published)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Published)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.Published)]
    public async Task GetRecordOverview_AnyRecordStatus_ReturnsRecordHeader(
        RecordStatus recordStatus,
        WorkflowStatus workflowStatus
    )
    {
        Record record = await AddRecord(recordStatus);
        await AddRevision(record, 1, workflowStatus);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordOverviewDto dto = result.ShouldBeSuccess();
        dto.RecordId.ShouldBe(record.Id);
        dto.OrganisationId.ShouldBe(_organisation.Id);
        dto.RecordStatus.ShouldBe(recordStatus);
        dto.ReviewedAt.ShouldBe(_reviewedAt);
    }

    [Theory]
    [InlineData(UserRole.Standard, true)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Super, false)]
    public async Task GetRecordOverview_CanEditContent_IsOnlyTrueForUsersWhoCanEditContent(
        UserRole role,
        bool expected
    )
    {
        Record record = await AddRecord(RecordStatus.Unpublished);
        await AddRevision(record, 1, WorkflowStatus.Draft);
        _harness.UpdateCurrentUser(x => x with { UserRole = role });

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess().CanEditContent.ShouldBe(expected);
    }

    [Theory]
    [InlineData(UserRole.Standard, true)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Super, false)]
    public async Task GetRecordOverview_ChangeStatus_IsOnlyReturnedToUsersWhoCanChangeStatus(
        UserRole role,
        bool expected
    )
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        _harness.UpdateCurrentUser(x => x with { UserRole = role });

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess().Actions.Contains(RecordAction.ChangeStatus).ShouldBe(expected);
    }

    [Fact]
    public async Task GetRecordOverview_DraftAfterPublishedRevision_ReturnsDraftRevisionTitle()
    {
        Record record = await AddRecord(RecordStatus.Published);
        RecordRevision published = await AddRevision(record, 1, WorkflowStatus.Published);
        RecordRevision draft = await AddRevision(record, 2, WorkflowStatus.Draft);
        await AddEntities(
            new[] { published, draft }.Select(r =>
                new RecordProductDetailFaker(RecordType.Medicine)
                    .RuleFor(x => x.RevisionId, r.Id)
                    .RuleFor(x => x.RecordTitle, $"Title {r.Id}")
                    .Generate()
            ),
            TestContext.Current.CancellationToken
        );

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordOverviewDto dto = result.ShouldBeSuccess();
        dto.RecordStatus.ShouldBe(RecordStatus.Published);
        dto.DisplayStatus.ShouldBe(RecordDisplayStatus.Draft);
        dto.RecordTitle.ShouldBe($"Title {draft.Id}");
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.Draft, RecordDisplayStatus.Draft)]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.QAReview, RecordDisplayStatus.QAReview)]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.Rejected, RecordDisplayStatus.Draft)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Draft, RecordDisplayStatus.Draft)]
    [InlineData(RecordStatus.Published, WorkflowStatus.QAReview, RecordDisplayStatus.QAReview)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Published, RecordDisplayStatus.Published)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Rejected, RecordDisplayStatus.Draft)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Draft, RecordDisplayStatus.OnHold)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Published, RecordDisplayStatus.OnHold)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.Draft, RecordDisplayStatus.Archived)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.Published, RecordDisplayStatus.Archived)]
    public async Task GetRecordOverview_ReturnsLatestRevisionWithDisplayStatus(
        RecordStatus recordStatus,
        WorkflowStatus latestWorkflowStatus,
        RecordDisplayStatus expectedDisplayStatus
    )
    {
        Record record = await AddRecord(recordStatus);
        await AddRevision(
            record,
            1,
            recordStatus == RecordStatus.Unpublished
                ? WorkflowStatus.Rejected
                : WorkflowStatus.Published
        );
        await AddRevision(record, 2, latestWorkflowStatus);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordOverviewDto dto = result.ShouldBeSuccess();
        dto.RecordStatus.ShouldBe(recordStatus);
        dto.DisplayStatus.ShouldBe(expectedDisplayStatus);
    }

    [Fact]
    public async Task GetRecordOverview_NoRevisions_ReturnsNotFound()
    {
        Record record = await AddRecord(RecordStatus.Unpublished);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
    }

    [Fact]
    public async Task GetRecordOverview_RecordDoesNotExist_ReturnsNotFound()
    {
        GetRecordResult result = await Service.GetRecordOverview(
            999_999,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
    }

    [Fact]
    public async Task GetRecordOverview_UserInAnotherOrganisation_ReturnsNotAllowed()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        var service = _harness
            .UpdateCurrentUser(x => x with { OrganisationId = _organisation.Id + 1 })
            .Service;

        GetRecordResult result = await service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotAllowed>();
    }

    [Fact]
    public async Task GetRecordOverview_RecordInAnotherOrganisation_ReturnsNotFound()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        var service = _harness
            .UpdateCurrentUser(x => x with { OrganisationId = otherOrganisation.Id })
            .Service;

        GetRecordResult result = await service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            otherOrganisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
    }

    [Fact]
    public async Task GetRecordOverview_DiscardedRecord_ReturnsNotFound()
    {
        Record record = await AddRecord(RecordStatus.Discarded);
        await AddRevision(record, 1, WorkflowStatus.Discarded);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
    }

    [Fact]
    public async Task GetRecordOverview_RecordNotInSelectableOrganisationThatIsNotSelected_ThrowsMismatch()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        var service = _harness
            .UpdateCurrentUser(x =>
                x with
                {
                    SelectableOrganisationIds = [_organisation.Id, otherOrganisation.Id],
                }
            )
            .Service;

        SelectedOrganisationMismatchException exception =
            await Should.ThrowAsync<SelectedOrganisationMismatchException>(() =>
                service.GetRecordOverview(
                    record.Id,
                    RecordType.Medicine,
                    otherOrganisation.Id,
                    TestContext.Current.CancellationToken
                )
            );
        exception.OrganisationId.ShouldBe(otherOrganisation.Id);
    }

    [Fact]
    public async Task GetRecordOverview_RecordInSelectableOrganisationThatIsNotSelected_ThrowsMismatch()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        Organisation selectedOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        var service = _harness
            .UpdateCurrentUser(x =>
                x with
                {
                    OrganisationId = selectedOrganisation.Id,
                    SelectableOrganisationIds = [selectedOrganisation.Id, _organisation.Id],
                }
            )
            .Service;

        SelectedOrganisationMismatchException exception =
            await Should.ThrowAsync<SelectedOrganisationMismatchException>(() =>
                service.GetRecordOverview(
                    record.Id,
                    RecordType.Medicine,
                    _organisation.Id,
                    TestContext.Current.CancellationToken
                )
            );
        exception.OrganisationId.ShouldBe(_organisation.Id);
    }

    [Theory]
    [InlineData(
        RecordStatus.Unpublished,
        new[] { WorkflowStatus.Draft },
        new[] { RecordAction.Update, RecordAction.Discard, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Unpublished,
        new[] { WorkflowStatus.QAReview },
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Unpublished,
        new[] { WorkflowStatus.Rejected, WorkflowStatus.Draft },
        new[] { RecordAction.Update, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Published,
        new[] { WorkflowStatus.Published },
        new[]
        {
            RecordAction.Update,
            RecordAction.ChangeStatus,
            RecordAction.ConfirmCurrent,
            RecordAction.ViewTimeline,
        }
    )]
    [InlineData(
        RecordStatus.Published,
        new[] { WorkflowStatus.Published, WorkflowStatus.QAReview },
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.OnHold,
        new[] { WorkflowStatus.Published, WorkflowStatus.Draft },
        new[] { RecordAction.ChangeStatus, RecordAction.ConfirmOnHold, RecordAction.ViewTimeline }
    )]
    public async Task GetRecordOverview_ReturnsPermittedActions(
        RecordStatus recordStatus,
        WorkflowStatus[] revisionStatuses,
        RecordAction[] expectedActions
    )
    {
        ArgumentNullException.ThrowIfNull(revisionStatuses);
        Record record = await AddRecord(recordStatus);
        await AddRevisions(record, revisionStatuses);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess().Actions.ShouldBe(expectedActions, ignoreOrder: true);
    }

    [Fact]
    public async Task GetRecordOverview_VaccineRecord_ReturnsVaccineRecord()
    {
        Record record = await AddRecord(RecordStatus.Published, RecordType.Vaccine);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Vaccine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordOverviewDto dto = result.ShouldBeSuccess();
        dto.RecordId.ShouldBe(record.Id);
        dto.RecordType.ShouldBe(RecordType.Vaccine);
        dto.CanEditContent.ShouldBeFalse();
    }

    [Theory]
    [InlineData(RecordType.Medicine, RecordType.Vaccine)]
    [InlineData(RecordType.Vaccine, RecordType.Medicine)]
    public async Task GetRecordOverview_RequestedTypeDiffers_ReturnsRecordTypeMismatch(
        RecordType actualRecordType,
        RecordType requestedRecordType
    )
    {
        Record record = await AddRecord(RecordStatus.Published, actualRecordType);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            requestedRecordType,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        var error = result.ShouldBeError().ShouldBeOfType<GetRecordError.RecordTypeMismatch>();
        error.RequestedRecordType.ShouldBe(requestedRecordType);
        error.ActualRecordType.ShouldBe(actualRecordType);
    }

    [Fact]
    public async Task GetRecordOverview_NoProductDetail_ReturnsNoTitle()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetRecordResult result = await Service.GetRecordOverview(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordOverviewDto dto = result.ShouldBeSuccess();
        dto.CompanyCode.ShouldBeNull();
        dto.RecordTitle.ShouldBeNull();
    }

    [Fact]
    public async Task GetRecordDetail_CompleteDraft_ReturnsOverviewAndAnswersThatApply()
    {
        Record record = await AddRecord(RecordStatus.Unpublished);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Draft);
        await CompleteMedicineRevision.AddAsync(
            Context,
            revision.Id,
            new(2027, 4, 1),
            TestContext.Current.CancellationToken
        );

        RecordDetailDto dto = (await GetDetail(record)).ShouldBeSuccess();

        dto.Overview.RecordId.ShouldBe(record.Id);
        Question(dto, Indication).Values.ShouldBe([IndicationAnswer]);
        Question(dto, IndicationIsCancer)
            .SelectedOptions.ShouldNotBeNull()
            .ShouldHaveSingleItem()
            .Label.ShouldBe("No");
        // Screening details only applies when screening is required, which it isn't.
        Questions(dto).Select(static q => q.Id).ShouldNotContain(ScreeningDetails);
    }

    [Theory]
    [InlineData(RecordStatus.Published, UserRole.Standard)]
    [InlineData(RecordStatus.Archived, UserRole.Standard)]
    [InlineData(RecordStatus.Published, UserRole.Super)]
    public async Task GetRecordDetail_RecordCheckYourAnswersRefuses_ReturnsAnswers(
        RecordStatus recordStatus,
        UserRole role
    )
    {
        Record record = await AddRecord(recordStatus);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);
        await CompleteMedicineRevision.AddAsync(
            Context,
            revision.Id,
            new(2027, 4, 1),
            TestContext.Current.CancellationToken
        );
        _harness.UpdateCurrentUser(x => x with { UserRole = role });

        RecordDetailDto dto = (await GetDetail(record)).ShouldBeSuccess();

        Question(dto, Indication).Values.ShouldBe([IndicationAnswer]);
    }

    [Fact]
    public async Task GetRecordDetail_NamesAndIdentifiers_ReturnsThemAsQuestions()
    {
        Record record = await AddRecord(RecordStatus.Published);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);
        await CompleteMedicineRevision.AddAsync(
            Context,
            revision.Id,
            new(2027, 4, 1),
            TestContext.Current.CancellationToken
        );

        RecordDetailDto dto = (await GetDetail(record)).ShouldBeSuccess();

        dto.Overview.CompanyCode.ShouldBe("ABC-123");
        dto.Overview.RecordTitle.ShouldBe("Chronic hepatitis C in adults");
        Question(dto, CreationPageFieldPaths.CompanyCode).Values.ShouldBe(["ABC-123"]);
        Question(dto, CreationPageFieldPaths.OtherIdentifiers).Values.ShouldBeEmpty();
        Question(dto, CreationPageFieldPaths.BrandedName).Values.ShouldBeEmpty();
        Question(dto, CreationPageFieldPaths.RecordTitle)
            .Values.ShouldBe(["Chronic hepatitis C in adults"]);
        RecordAnswersQuestionDto genericNames = Question(dto, CreationPageFieldPaths.GenericNames);
        genericNames.Values.ShouldBe(["sofosbuvir"]);
        // A text list has its own item label, which is only sent for a group.
        genericNames.Items.ShouldBeNull();
        genericNames.ItemLabel.ShouldBeNull();
        genericNames.CheckAnswersAddLabel.ShouldBeNull();
    }

    [Fact]
    public async Task GetRecordDetail_InvalidAnswers_ReturnsThemUnchecked()
    {
        // Screening details is required once screening is, but viewing doesn't check answers.
        Record record = await AddRecord(RecordStatus.Unpublished);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Draft);
        await CompleteMedicineRevision.AddAsync(
            Context,
            revision.Id,
            new(2027, 4, 1),
            TestContext.Current.CancellationToken
        );
        MedicinesPatientIdentification identification =
            await Context.MedicinesPatientIdentifications.SingleAsync(
                x => x.RevisionId == revision.Id,
                TestContext.Current.CancellationToken
            );
        identification.ScreeningRequired = YesNoUnknown.Yes;
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Context.ChangeTracker.Clear();

        RecordDetailDto dto = (await GetDetail(record)).ShouldBeSuccess();

        Question(dto, ScreeningDetails).Values.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRecordDetail_ClinicalTrials_ReturnsEachTrialAsAGroupItem()
    {
        Record record = await AddRecord(RecordStatus.Unpublished);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Draft);
        await CompleteMedicineRevision.AddAsync(
            Context,
            revision.Id,
            new(2027, 4, 1),
            TestContext.Current.CancellationToken
        );
        await CompleteMedicineRevision.AddClinicalTrialAsync(
            Context,
            revision.Id,
            TestContext.Current.CancellationToken
        );

        RecordDetailDto dto = (await GetDetail(record)).ShouldBeSuccess();

        RecordAnswersQuestionDto trials = Question(dto, ClinicalTrials);
        trials.Values.ShouldBeEmpty();
        trials.ItemLabel.ShouldBe("Clinical trial");
        trials.CheckAnswersAddLabel.ShouldBe("Add clinical trial");
        var item = trials.Items.ShouldNotBeNull().ShouldHaveSingleItem();
        item.Questions.Select(static q => q.Id)
            .ShouldBe(["study_name", "clinical_trials_gov_number", "other_registry_numbers"]);
        item.Questions.ShouldAllBe(q => q.PageId == trials.PageId);
        item.Questions[0].Values.ShouldBe(["SURVEYOR-1"]);
        item.Questions[1].Values.ShouldBe(["NCT01234567"]);
        item.Questions[2].Values.ShouldBe(["ISRCTN12345678"]);

        // A text list has its own item label, which is only sent for a group.
        RecordAnswersQuestionDto registryNumbers = item.Questions[2];
        registryNumbers.Items.ShouldBeNull();
        registryNumbers.ItemLabel.ShouldBeNull();
        registryNumbers.CheckAnswersAddLabel.ShouldBeNull();
    }

    [Fact]
    public async Task GetRecordDetail_VaccineRecord_ReturnsNoSections()
    {
        Record record = await AddRecord(RecordStatus.Published, RecordType.Vaccine);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetRecordDetailResult result = await Service.GetRecordDetail(
            record.Id,
            RecordType.Vaccine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

        RecordDetailDto dto = result.ShouldBeSuccess();
        dto.Overview.RecordType.ShouldBe(RecordType.Vaccine);
        dto.Sections.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRecordDetail_RecordInAnotherOrganisation_ReturnsNotFound()
    {
        Record record = await AddRecord(RecordStatus.Published);
        await AddRevision(record, 1, WorkflowStatus.Published);
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        var service = _harness
            .UpdateCurrentUser(x => x with { OrganisationId = otherOrganisation.Id })
            .Service;

        GetRecordDetailResult result = await service.GetRecordDetail(
            record.Id,
            RecordType.Medicine,
            otherOrganisation.Id,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
    }

    private Task<GetRecordDetailResult> GetDetail(Record record) =>
        Service.GetRecordDetail(
            record.Id,
            RecordType.Medicine,
            _organisation.Id,
            TestContext.Current.CancellationToken
        );

    private static IEnumerable<RecordAnswersQuestionDto> Questions(RecordDetailDto dto) =>
        dto.Sections.SelectMany(static s => s.Tasks).SelectMany(static t => t.Questions);

    private static RecordAnswersQuestionDto Question(RecordDetailDto dto, string id) =>
        Questions(dto).Single(q => string.Equals(q.Id, id, StringComparison.Ordinal));

    private async Task<Record> AddRecord(
        RecordStatus status,
        RecordType recordType = RecordType.Medicine
    ) =>
        await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, _organisation.Id)
                .RuleFor(x => x.RecordType, recordType)
                .RuleFor(x => x.RecordStatus, status)
                .RuleFor(x => x.ReviewedAt, _reviewedAt)
                .Generate(),
            TestContext.Current.CancellationToken
        );

    private async Task<RecordRevision> AddRevision(
        Record record,
        int sequence,
        WorkflowStatus workflowStatus
    ) =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, workflowStatus)
                .RuleFor(x => x.CreatedAt, _reviewedAt.AddDays(sequence))
                .Generate(),
            TestContext.Current.CancellationToken
        );

    private async Task AddRevisions(Record record, WorkflowStatus[] workflowStatuses)
    {
        for (int i = 0; i < workflowStatuses.Length; i++)
        {
            await AddRevision(record, i + 1, workflowStatuses[i]);
        }
    }
}
