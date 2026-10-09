using Shouldly;
using UKPS.Api.Persistence.Data.Seeding;
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
        foreach (RecordRevision revision in Payload.Records.SelectMany(r => r.Revisions))
        {
            revision.UpdatedAt?.ShouldBeGreaterThanOrEqualTo(revision.CreatedAt);
            revision.SubmittedAt?.ShouldBeGreaterThanOrEqualTo(
                revision.UpdatedAt ?? revision.CreatedAt
            );
            foreach (QaReview review in revision.QaReviews)
            {
                review.ReviewedAt!.Value.ShouldBeGreaterThan(revision.SubmittedAt!.Value);
            }

            QaOutcome? outcome = revision.QaReviews.SingleOrDefault()?.Outcome;
            WorkflowStatus expected = outcome switch
            {
                QaOutcome.Approved => WorkflowStatus.Published,
                QaOutcome.Rejected => WorkflowStatus.Rejected,
                _ when revision.SubmittedAt is not null => WorkflowStatus.QAReview,
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
            if (record.RecordStatus == RecordStatus.Unpublished)
            {
                published.ShouldBeEmpty();
                record.ReviewedAt.ShouldBeNull();
                continue;
            }

            published.ShouldNotBeEmpty();
            DateTime lastSubmitted = published.Max(r => r.SubmittedAt!.Value);
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
    public void Updates_ShouldRecordTheFieldsTheyChanged()
    {
        RecordEventFieldChange[] changes =
        [
            .. Payload
                .Records.SelectMany(r =>
                    r.Events.Where(e => e.EventType == RecordEventType.RecordPublished).Skip(1)
                )
                .SelectMany(e =>
                {
                    e.FieldChanges.ShouldNotBeEmpty();
                    return e.FieldChanges;
                }),
        ];

        changes.ShouldNotBeEmpty();
        changes.ShouldAllBe(c => c.OldValue != c.NewValue);
        changes.ShouldContain(c =>
            c.OldValue == nameof(YesNoUnknown.Unknown) && c.NewValue != nameof(YesNoUnknown.Unknown)
        );
    }

    [Fact]
    public void BuildPayload_ShouldGiveTheSameRecordHistoryEachTime()
    {
        SeedingDataPayload again = DataSeederInMemory.BuildPayload(new SeedingOptions());

        static IEnumerable<string> History(SeedingDataPayload payload) =>
            payload.Records.SelectMany(r =>
                r.Events.Select(e => $"{e.EventType} {e.PerformedAt:O} {e.FieldChanges.Count}")
            );

        History(again).ShouldBe(History(Payload));
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
