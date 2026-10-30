using System.Text.RegularExpressions;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.Persistence.Data.Seeding.SyntheticData;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence;

public sealed class SeededRecordHistoryTests
{
    private static readonly Lazy<SeedingDataPayload> _payload = new(() =>
        DataSeederInMemory.BuildPayload(new SeedingOptions())
    );

    private static readonly Type[] _onePerRevision =
    [
        typeof(RecordProductDetail),
        typeof(RecordClinicalTrialInformation),
        typeof(RecordMhraProcedure),
        typeof(RecordMhraDate),
        typeof(RecordHta),
        typeof(MedicinesIndicationDetail),
        typeof(MedicinesDevelopmentBackground),
        typeof(MedicinesGlobalSubmission),
        typeof(MedicinesIntlRecognition),
        typeof(MedicinesEuStatus),
        typeof(MedicinesEamsPim),
        typeof(MedicinesLaboratoryTesting),
        typeof(MedicinesPatientIdentification),
        typeof(MedicinesTreatmentDetail),
        typeof(MedicinesServiceImpact),
        typeof(MedicinesBudgetImpact),
    ];

    private static SeedingDataPayload Payload => _payload.Value;

    [Fact]
    public void Organisations_ShouldHaveEachTypeWithTheInternalOrganisationFirst()
    {
        Payload.Organisations.First().OrganisationType.ShouldBe(OrganisationType.Internal);
        Payload
            .Organisations.CountBy(o => o.OrganisationType)
            .ToDictionary()
            .ShouldBe(
                new Dictionary<OrganisationType, int>
                {
                    [OrganisationType.Internal] = 1,
                    [OrganisationType.PharmaCompany] = 35,
                    [OrganisationType.HorizonScanning] = 10,
                    [OrganisationType.Strategic] = 4,
                },
                ignoreOrder: true
            );
    }

    [Fact]
    public void Users_ShouldMatchTheirOrganisationTypeAndHaveUniqueEmails()
    {
        Payload.Users.Select(u => u.WorkEmail).ShouldBeUnique(StringComparer.OrdinalIgnoreCase);
        foreach (UserOrgMembership membership in Payload.Memberships.SkipLast(1))
        {
            UserType expected = membership.Organisation!.OrganisationType switch
            {
                OrganisationType.Internal => UserType.QaUser,
                OrganisationType.PharmaCompany => UserType.PharmaUser,
                OrganisationType.HorizonScanning => UserType.HorizonScanner,
                _ => UserType.StrategicUser,
            };
            membership.User!.UserType.ShouldBe(expected);
        }
    }

    [Fact]
    public void Records_ShouldBeEditedByActiveMembersAndReviewedByQaUsers()
    {
        ILookup<Organisation, User> activeMembers = Payload
            .Memberships.Where(m => m.Status == UserOrgMembershipStatus.Active)
            .ToLookup(m => m.Organisation!, m => m.User!);

        foreach (Record record in Payload.Records)
        {
            record
                .Organisation.ShouldNotBeNull()
                .OrganisationType.ShouldBe(OrganisationType.PharmaCompany);
            foreach (RecordRevision revision in record.Revisions)
            {
                activeMembers[record.Organisation].ShouldContain(revision.CreatedByUser!);
                revision.QaReviews.ShouldAllBe(r => r.ReviewedByUser!.UserType == UserType.QaUser);
            }
            record.CreatedByUser.ShouldBeSameAs(record.Revisions.First().CreatedByUser);
        }
    }

    [Fact]
    public void Records_ShouldHaveEventsInChronologicalOrderStartingWithCreation()
    {
        foreach (Record record in Payload.Records)
        {
            record.Events.First().EventType.ShouldBe(RecordEventType.RecordCreated);
            record.Events.First().PerformedAt.ShouldBe(record.CreatedAt);
            record.Events.Select(e => e.PerformedAt).ShouldBeInOrder();
            record.StatusHistory.Select(h => h.UpdatedAt).ShouldBeInOrder();
            record.StatusHistory.First().ToStatus.ShouldBe(RecordStatus.Unpublished);
            record.StatusHistory.Last().ToStatus.ShouldBe(record.RecordStatus);
        }
    }

    [Fact]
    public void Revisions_ShouldMoveThroughTheWorkflowInOrder()
    {
        ILookup<RecordRevision, RecordFieldAudit> audits = AuditsByRevision();
        foreach (RecordRevision revision in Payload.Records.SelectMany(r => r.Revisions))
        {
            DateTime? lastSaved = audits[revision].Select(a => (DateTime?)a.ChangedAt).Max();
            DateTime? submitted = SubmittedAt(revision);
            lastSaved?.ShouldBeGreaterThan(revision.CreatedAt);
            submitted?.ShouldBeGreaterThan(lastSaved ?? revision.CreatedAt);
            foreach (QaReview review in revision.QaReviews)
            {
                review.ReviewedAt!.Value.ShouldBeGreaterThan(submitted!.Value);
            }

            QaOutcome? outcome = revision.QaReviews.SingleOrDefault()?.Outcome;
            WorkflowStatus expected = outcome switch
            {
                QaOutcome.Approved => WorkflowStatus.Published,
                QaOutcome.Rejected => WorkflowStatus.Rejected,
                _ when submitted is not null => WorkflowStatus.QAReview,
                _ when revision.Record!.RecordStatus == RecordStatus.Discarded =>
                    WorkflowStatus.Discarded,
                _ => WorkflowStatus.Draft,
            };
            revision.WorkflowStatus.ShouldBe(expected);
        }
    }

    [Fact]
    public void PublishedRecords_ShouldHaveAPublishedRevisionAndReviewDate()
    {
        foreach (Record record in Payload.Records)
        {
            RecordRevision[] published =
            [
                .. record.Revisions.Where(r => r.WorkflowStatus == WorkflowStatus.Published),
            ];
            if (record.RecordStatus is RecordStatus.Unpublished or RecordStatus.Discarded)
            {
                published.ShouldBeEmpty();
                record.ReviewedAt.ShouldBeNull();
                continue;
            }

            published.ShouldNotBeEmpty();
            DateTime lastSubmitted = published.Max(r => SubmittedAt(r)!.Value);
            record.ReviewedAt.ShouldNotBeNull().ShouldBeGreaterThanOrEqualTo(lastSubmitted);
        }
    }

    [Fact]
    public void Revisions_ShouldEachHaveTheirOwnContent()
    {
        ILookup<RecordRevision, object> content = Payload
            .SeededRecords.SelectMany(r => r.RevisionContent)
            .ToLookup(RevisionOf);

        foreach (RecordRevision revision in Payload.Records.SelectMany(r => r.Revisions))
        {
            object[] sections = [.. content[revision]];
            sections.OfType<RecordProductDetail>().Count().ShouldBe(1);
            sections.OfType<MedicinesIndicationDetail>().Count().ShouldBe(1);
            foreach (Type type in _onePerRevision)
            {
                sections.Count(type.IsInstanceOfType).ShouldBeLessThanOrEqualTo(1);
            }

            RegulatoryDate[] dates = [.. sections.SelectMany(RegulatoryDatesOf)];
            dates.ShouldAllBe(d => ReferenceEquals(d.Revision, revision));
            dates.Select(d => d.DateEvent).ShouldBeUnique();
        }
    }

    [Fact]
    public void AutomaticArchives_ShouldBeRecordedAsSystemChanges()
    {
        Record[] archived =
        [
            .. Payload.Records.Where(r =>
                r.StatusHistory.Last().Reason == RecordStatusChangeReason.ArchivedAutomatically
            ),
        ];

        archived.ShouldNotBeEmpty();
        foreach (Record record in archived)
        {
            RecordStatusHistory change = record.StatusHistory.Last();
            record.RecordStatus.ShouldBe(RecordStatus.Archived);
            change.UpdatedByUser.ShouldBeNull();
            change.Note.ShouldStartWith("Archived automatically:");
            record
                .Events.Last(e => e.EventType == RecordEventType.RecordStatusChanged)
                .PerformedByUser.ShouldBeNull();

            // Archiving is not a review.
            record.ReviewedAt.ShouldNotBeNull().ShouldBeLessThan(change.UpdatedAt);
        }
        Payload
            .Records.SelectMany(r => r.StatusHistory)
            .Where(h => h.Reason == RecordStatusChangeReason.ArchivedAutomatically)
            .ShouldAllBe(h => h.ToStatus == RecordStatus.Archived);
    }

    [Fact]
    public void AutomaticArchives_ForRecordsNotUpdated_ShouldFollowAYearWithoutReview()
    {
        Record[] notUpdated =
        [
            .. Payload.Records.Where(r =>
                r.StatusHistory.Last().Note?.Contains("not been updated", StringComparison.Ordinal)
                == true
            ),
        ];

        notUpdated.Length.ShouldBe(10);
        foreach (Record record in notUpdated)
        {
            DateTime archivedAt = record.StatusHistory.Last().UpdatedAt;
            (archivedAt - record.ReviewedAt!.Value).TotalDays.ShouldBeGreaterThan(365);
        }
    }

    [Fact]
    public void FieldAudits_ShouldChainEachSaveFromTheValueLeftByThePreviousOne()
    {
        foreach (SeededRecord seeded in Payload.SeededRecords)
        {
            seeded.FieldAudits.Select(a => a.ChangedAt).ShouldBeInOrder();
            Dictionary<string, string?> saved = new(StringComparer.Ordinal);
            foreach (RecordFieldAudit audit in seeded.FieldAudits)
            {
                audit.Record.ShouldBeSameAs(seeded.Record);
                audit.NewValue.ShouldNotBe(audit.OldValue);
                audit.OldValue.ShouldBe(saved.GetValueOrDefault(audit.FieldPath));
                saved[audit.FieldPath] = audit.NewValue;
            }
        }
    }

    [Fact]
    public void FieldAudits_ShouldBeMadeByActiveMembersWhileTheRevisionIsADraft()
    {
        ILookup<Organisation, User> activeMembers = Payload
            .Memberships.Where(m => m.Status == UserOrgMembershipStatus.Active)
            .ToLookup(m => m.Organisation!, m => m.User!);

        foreach (SeededRecord seeded in Payload.SeededRecords)
        {
            foreach (RecordFieldAudit audit in seeded.FieldAudits)
            {
                activeMembers[seeded.Record.Organisation!].ShouldContain(audit.ChangedByUser!);
                audit.ChangedAt.ShouldBeGreaterThanOrEqualTo(audit.Revision!.CreatedAt);
                audit.ChangedAt.ShouldBeLessThan(SubmittedAt(audit.Revision) ?? DateTime.MaxValue);
            }
        }
    }

    [Fact]
    public void FieldAudits_ShouldRecordTheFirstSaveOfEveryRecordAndEachChangeBeforePublishing()
    {
        ILookup<RecordRevision, RecordFieldAudit> audits = AuditsByRevision();
        foreach (Record record in Payload.Records)
        {
            audits[record.Revisions.First()].ShouldNotBeEmpty();
            foreach (RecordRevision revision in record.Revisions.Skip(1))
            {
                if (revision.WorkflowStatus != WorkflowStatus.Draft)
                {
                    audits[revision].ShouldNotBeEmpty();
                }
            }
        }
    }

    [Fact]
    public void FieldAudits_ShouldRecordTheCreationPageWhenTheRecordIsCreated()
    {
        ILookup<RecordRevision, RecordFieldAudit> audits = AuditsByRevision();
        foreach (Record record in Payload.Records)
        {
            RecordRevision first = record.Revisions.First();
            RecordProductDetail productDetail = Payload
                .SeededRecords.Single(r => r.Record == record)
                .RevisionContent.OfType<RecordProductDetail>()
                .First(d => d.Revision == first);
            string[] NamesOfType(NameAndIdentifierType type) =>
                [
                    .. productDetail
                        .NamesAndIdentifiers.Where(n => n.NameType == type)
                        .OrderBy(n => n.DisplayOrder)
                        .Select(n => n.Name),
                ];

            Dictionary<string, string?> expected = new(StringComparer.Ordinal)
            {
                ["record_product_detail.company_code"] = productDetail.CompanyCode,
                ["record_product_detail.record_title"] = productDetail.RecordTitle,
                ["record_name_and_identifier"] = RecordNameAndIdentifier.AuditText(
                    NamesOfType(NameAndIdentifierType.GenericName),
                    NamesOfType(NameAndIdentifierType.OtherIdentifier)
                ),
            };
            if (productDetail.BrandedName is not null)
            {
                expected["record_product_detail.branded_name"] = productDetail.BrandedName;
            }

            audits[first]
                .Where(a => a.ChangedAt == record.CreatedAt)
                .ToDictionary(a => a.FieldPath, a => a.NewValue, StringComparer.Ordinal)
                .ShouldBe(expected, ignoreOrder: true);
        }
    }

    [Fact]
    public void AuditedFields_ShouldIncludeTheIdOfEveryMedicineFormQuestion()
    {
        FormModel model = FormModelFactory
            .Create(OptionProviderRegistry.CreateDefault())
            .Find(RecordType.Medicine)!;
        IReadOnlyDictionary<string, string?> audited = RecordContentHistory.AuditedFields(
            SyntheticDataLoader.DataSet.Records[0]
        );

        model.Pages.SelectMany(p => p.Questions).Select(q => q.Id).ShouldBeSubsetOf(audited.Keys);
    }

    [Fact]
    public void FieldAudits_ForDates_ShouldHoldTheDatesContentsAsJson()
    {
        Regex format = new(
            $$"""^\{"date_precision":"({{string.Join('|', Enum.GetNames<DatePrecision>())}})","date_value":"\d{4}-\d{2}-\d{2}","is_confidential":(true|false)\}$""",
            RegexOptions.None,
            TimeSpan.FromSeconds(1)
        );
        string[] values =
        [
            .. Payload
                .SeededRecords.SelectMany(r => r.FieldAudits)
                .Where(a => a.FieldPath.EndsWith("_date", StringComparison.Ordinal))
                .SelectMany(a => new[] { a.OldValue, a.NewValue })
                .OfType<string>(),
        ];

        values.ShouldNotBeEmpty();
        values.ShouldAllBe(v => format.IsMatch(v));
    }

    [Fact]
    public void FieldAudits_ShouldIncludeProvisionalValuesCorrectedWithinTheSameDraft()
    {
        Payload
            .SeededRecords.SelectMany(r => r.FieldAudits)
            .GroupBy(a => (a.Revision, a.FieldPath))
            .ShouldContain(changes => changes.Count() > 1);
    }

    [Fact]
    public void FieldAudits_AfterARejection_ShouldCorrectWhatQaSentBack()
    {
        ILookup<RecordRevision, RecordFieldAudit> audits = AuditsByRevision();
        RecordRevision[] reworks =
        [
            .. Payload
                .Records.SelectMany(r => r.Revisions)
                .Where(r => r.BasedOnRevision?.WorkflowStatus == WorkflowStatus.Rejected),
        ];

        reworks.ShouldNotBeEmpty();
        foreach (RecordRevision rework in reworks)
        {
            IReadOnlyDictionary<string, string?> rejected = ValuesAfter(
                audits,
                rework.BasedOnRevision!
            );
            RecordFieldAudit[] corrections = [.. audits[rework]];
            corrections.ShouldNotBeEmpty();
            corrections.ShouldAllBe(c => c.OldValue == rejected.GetValueOrDefault(c.FieldPath));
        }
    }

    [Fact]
    public void FieldAudits_ForPublishedUpdates_ShouldShowFieldsDisclosedLater()
    {
        Payload
            .SeededRecords.SelectMany(r => r.FieldAudits)
            .Where(a => a.Revision!.BasedOnRevision is not null)
            .ShouldContain(a =>
                a.OldValue == nameof(YesNoUnknown.Unknown)
                && a.NewValue != nameof(YesNoUnknown.Unknown)
            );
    }

    [Fact]
    public void DiscardedRecords_ShouldOnlyHaveHadASingleDraftRevision()
    {
        foreach (
            Record record in Payload.Records.Where(r => r.RecordStatus == RecordStatus.Discarded)
        )
        {
            RecordRevision revision = record.Revisions.ShouldHaveSingleItem();
            revision.WorkflowStatus.ShouldBe(WorkflowStatus.Discarded);
            revision.QaReviews.ShouldBeEmpty();
            SubmittedAt(revision).ShouldBeNull();
            record.ReviewedAt.ShouldBeNull();
        }
    }

    [Fact]
    public void DiscardedRecords_ShouldRecordTheDiscardInTheStatusHistoryAndEvents()
    {
        ILookup<Organisation, User> activeMembers = Payload
            .Memberships.Where(m => m.Status == UserOrgMembershipStatus.Active)
            .ToLookup(m => m.Organisation!, m => m.User!);
        ILookup<RecordRevision, RecordFieldAudit> audits = AuditsByRevision();

        foreach (
            Record record in Payload.Records.Where(r => r.RecordStatus == RecordStatus.Discarded)
        )
        {
            record.StatusHistory.Count.ShouldBe(2);
            RecordStatusHistory history = record.StatusHistory.Last();
            history.FromStatus.ShouldBe(RecordStatus.Unpublished);
            history.ToStatus.ShouldBe(RecordStatus.Discarded);
            history.Reason.ShouldBe(RecordStatusChangeReason.ManuallyDiscarded);
            activeMembers[record.Organisation!].ShouldContain(history.UpdatedByUser!);

            RecordEvent discard = record.Events.Last();
            discard.EventType.ShouldBe(RecordEventType.RecordStatusChanged);
            discard.Revision.ShouldBeSameAs(record.Revisions.Single());
            discard.PerformedByUser.ShouldBeSameAs(history.UpdatedByUser);
            discard.PerformedAt.ShouldBe(history.UpdatedAt);
            discard.PerformedAt.ShouldBeGreaterThan(
                audits[record.Revisions.Single()].Max(a => a.ChangedAt)
            );
            discard.PerformedAt.ShouldBeLessThanOrEqualTo(DataSetToday());
        }
    }

    [Fact]
    public void BuildPayload_ShouldGiveTheSameRecordHistoryEachTime()
    {
        SeedingDataPayload again = DataSeederInMemory.BuildPayload(new SeedingOptions());

        static IEnumerable<string> History(SeedingDataPayload payload) =>
            payload.SeededRecords.SelectMany(r =>
                r.Record.Events.Select(e => $"{e.EventType} {e.PerformedAt:O}")
                    .Concat(
                        r.FieldAudits.Select(a =>
                            $"{a.FieldPath} {a.ChangedAt:O} {a.OldValue} -> {a.NewValue}"
                        )
                    )
            );

        History(again).ShouldBe(History(Payload));
    }

    private static DateTime DataSetToday() =>
        SyntheticDataLoader.DataSet.AsOf.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private static ILookup<RecordRevision, RecordFieldAudit> AuditsByRevision() =>
        Payload.SeededRecords.SelectMany(r => r.FieldAudits).ToLookup(a => a.Revision!);

    private static DateTime? SubmittedAt(RecordRevision revision) =>
        revision
            .Record!.Events.Where(e =>
                ReferenceEquals(e.Revision, revision)
                && e.EventType == RecordEventType.SubmittedToQa
            )
            .Select(e => (DateTime?)e.PerformedAt)
            .LastOrDefault();

    /// <summary>The audited field values as they stood when <paramref name="revision"/> was last saved.</summary>
    private static Dictionary<string, string?> ValuesAfter(
        ILookup<RecordRevision, RecordFieldAudit> audits,
        RecordRevision revision
    )
    {
        Dictionary<string, string?> values = new(StringComparer.Ordinal);
        for (
            RecordRevision? current = revision;
            current is not null;
            current = current.BasedOnRevision
        )
        {
            foreach (RecordFieldAudit audit in audits[current].Reverse())
            {
                values.TryAdd(audit.FieldPath, audit.NewValue);
            }
        }
        return values;
    }

    private static RecordRevision RevisionOf(object content) =>
        (RecordRevision)content.GetType().GetProperty("Revision")!.GetValue(content)!;

    private static IEnumerable<RegulatoryDate> RegulatoryDatesOf(object content) =>
        content
            .GetType()
            .GetProperties()
            .Where(p => p.PropertyType == typeof(RegulatoryDate))
            .Select(p => (RegulatoryDate?)p.GetValue(content))
            .OfType<RegulatoryDate>();
}
