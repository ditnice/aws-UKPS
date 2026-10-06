using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using static UKPS.Api.Persistence.Data.Seeding.SyntheticData.SyntheticRecord;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>Builds the history of a single synthetic record.</summary>
internal sealed class RecordHistoryBuilder
{
    private const double FirstSubmissionRejectionChance = 0.2;
    private const double UpdateRejectionChance = 0.08;
    private const double PendingDraftChance = 0.12;
    private const double InitiallyUnknownChance = 0.4;

    private static readonly string[] _rejectionNotes =
    [
        "Please confirm the UK submission date and whether it is an estimate.",
        "The indication wording does not match the clinical trial population. Please review.",
        "Please add the reference regulator for the International Recognition Procedure.",
        "Proposed place in therapy needs likely comparators.",
        "Please check the BNF chapter against the mode of action.",
    ];

    private readonly SyntheticRecord _source;
    private readonly IReadOnlyList<User> _authors;
    private readonly IReadOnlyList<User> _reviewers;
    private readonly RecordContentFactory _contentFactory;
    private readonly DateOnly _today;
    private readonly Randomizer _random;
    private readonly List<object> _content = [];
    private readonly Record _record;
    private User _author;

    public RecordHistoryBuilder(
        SyntheticRecord source,
        Organisation organisation,
        IReadOnlyList<User> authors,
        IReadOnlyList<User> reviewers,
        RecordContentFactory contentFactory,
        DateOnly today,
        Randomizer random
    )
    {
        _source = source;
        _authors = authors;
        _reviewers = reviewers;
        _contentFactory = contentFactory;
        _today = today;
        _random = random;
        _author = Pick(authors);
        _record = new Record
        {
            Organisation = organisation,
            RecordType = source.RecordType,
            CreatedAt = At(source.CreatedAt, 9, 11),
            CreatedByUser = _author,
        };
    }

    public SeededRecord Build()
    {
        // New records start unpublished.
        _record.StatusHistory.Add(
            new RecordStatusHistory
            {
                Record = _record,
                ToStatus = RecordStatus.Unpublished,
                UpdatedAt = _record.CreatedAt,
                UpdatedByUser = _author,
            }
        );
        RecordRevision first = CreateRevision(_record.CreatedAt, basedOn: null);

        if (_source.RecordStatus == RecordStatus.Unpublished)
        {
            BuildDraftHistory(first);
        }
        else
        {
            BuildPublishedHistory(first);
        }

        return new SeededRecord(_record, _content);
    }

    private void BuildDraftHistory(RecordRevision first)
    {
        DateOnly lastUpdated = _source.LastUpdatedAt;
        double outcome = _random.Double();
        bool canBeRejected = lastUpdated.DayNumber - _source.CreatedAt.DayNumber >= 2;

        if (outcome < 0.2 && canBeRejected)
        {
            // Submitted once, sent back by QA, and now being reworked.
            DateOnly submitted = _source.CreatedAt.AddDays(1);
            Submit(first, At(submitted, 10, 12));
            DateOnly rejected = submitted.AddDays(1);
            Reject(first, At(rejected, 9, 11));
            AddContent(first, _source);

            RecordRevision rework = CreateRevision(At(rejected, 11, 13), basedOn: first);
            Edit(rework, At(lastUpdated, 14, 17));
            AddContent(rework, _source);
            return;
        }

        Edit(first, At(lastUpdated, 12, 15));
        if (outcome < 0.45)
        {
            Submit(first, At(lastUpdated, 15, 17));
        }
        AddContent(first, _source);
    }

    private void BuildPublishedHistory(RecordRevision first)
    {
        IReadOnlyList<DateOnly> publishDays = PlanPublishDays(
            _source.CreatedAt,
            _source.LastUpdatedAt
        );
        IReadOnlyDictionary<string, int> disclosedAt = PlanDisclosures(publishDays.Count);
        int maximumDriftMonths = _random.Number(1, 4);

        SyntheticRecord? published = null;
        RecordRevision? publishedRevision = null;
        for (int index = 0; index < publishDays.Count; index++)
        {
            SyntheticRecord version = VersionAt(
                index,
                publishDays,
                maximumDriftMonths,
                disclosedAt
            );
            if (published is null || publishedRevision is null)
            {
                publishedRevision = Publish(
                    first,
                    _source.CreatedAt,
                    publishDays[index],
                    version,
                    []
                );
                _record.ChangeStatus(
                    RecordStatus.Active,
                    _record.ReviewedAt!.Value,
                    publishedRevision.QaReviews.Last().ReviewedByUser
                );
                published = version;
                continue;
            }

            var changes = RecordContentHistory.Changes(published, version);
            if (changes.Count == 0)
            {
                ReviewWithoutChange(At(publishDays[index], 10, 16), publishedRevision);
                continue;
            }

            if (_random.Double() < 0.3)
            {
                _author = Pick(_authors);
            }
            DateOnly draftDay = DayBetween(publishDays[index - 1].AddDays(1), publishDays[index]);
            RecordRevision draft = CreateRevision(At(draftDay, 9, 11), basedOn: publishedRevision);
            publishedRevision = Publish(draft, draftDay, publishDays[index], version, changes);
            published = version;
        }

        FinishPublishedHistory(publishedRevision!);
    }

    private void FinishPublishedHistory(RecordRevision publishedRevision)
    {
        if (_source.StatusChange is { } statusChange)
        {
            ChangeStatus(statusChange, At(statusChange.ChangedAt, 15, 17));
        }
        else if (_source.ReviewedAt is { } reviewed && reviewed > _source.LastUpdatedAt)
        {
            ReviewWithoutChange(At(reviewed, 10, 16), publishedRevision);
        }

        DateOnly lastActivity = DateOnly.FromDateTime(_record.ReviewedAt!.Value);
        bool hasTimeForDraft = _today.DayNumber - lastActivity.DayNumber >= 7;
        if (
            _source.RecordStatus == RecordStatus.Active
            && hasTimeForDraft
            && _random.Double() < PendingDraftChance
        )
        {
            // A further update is being prepared but has not been published yet.
            DateOnly draftDay = DayBetween(lastActivity.AddDays(1), _today.AddDays(-1));
            RecordRevision draft = CreateRevision(At(draftDay, 9, 11), basedOn: publishedRevision);
            Edit(draft, At(draftDay, 11, 14));
            if (_random.Double() < 0.3)
            {
                Submit(draft, At(draftDay, 14, 17));
            }
            AddContent(draft, _source);
        }
    }

    private SyntheticRecord VersionAt(
        int index,
        IReadOnlyList<DateOnly> publishDays,
        int maximumDriftMonths,
        IReadOnlyDictionary<string, int> disclosedAt
    )
    {
        int last = publishDays.Count - 1;
        if (index == last)
        {
            return _source;
        }

        // Earlier estimates were further from what eventually happened.
        int drift = (int)Math.Round(maximumDriftMonths * (double)(last - index) / last);
        HashSet<string> unknown = disclosedAt
            .Where(field => field.Value > index)
            .Select(field => field.Key)
            .ToHashSet(StringComparer.Ordinal);
        return RecordContentHistory.AsKnownAt(_source, publishDays[index], drift, unknown);
    }

    /// <summary>Picks the revision from which some Yes/No answers stopped being Unknown.</summary>
    private Dictionary<string, int> PlanDisclosures(int publicationCount)
    {
        Dictionary<string, int> disclosedAt = new(StringComparer.Ordinal);
        if (publicationCount < 2)
        {
            return disclosedAt;
        }

        IReadOnlyDictionary<string, string?> fields = RecordContentHistory.TrackedFields(_source);
        foreach (string field in RecordContentHistory.DisclosableFields)
        {
            bool known = fields[field] is nameof(YesNoUnknown.Yes) or nameof(YesNoUnknown.No);
            if (known && _random.Double() < InitiallyUnknownChance)
            {
                disclosedAt[field] = _random.Number(1, publicationCount - 1);
            }
        }
        return disclosedAt;
    }

    private IReadOnlyList<DateOnly> PlanPublishDays(DateOnly created, DateOnly final)
    {
        int span = final.DayNumber - created.DayNumber;
        if (span < 14)
        {
            return [final];
        }

        int updates = _random.Number(1, Math.Min(4, 1 + (span / 120)));
        DateOnly first = created.AddDays(_random.Number(1, Math.Min(21, span / 2)));
        SortedSet<DateOnly> days = [first, final];
        int available = final.DayNumber - first.DayNumber - 1;
        while (days.Count < updates + 1 && days.Count - 2 < available)
        {
            days.Add(first.AddDays(_random.Number(1, available)));
        }
        return [.. days];
    }

    /// <summary>Takes a revision through QA and publishes it on <paramref name="publishDay"/>.</summary>
    private RecordRevision Publish(
        RecordRevision draft,
        DateOnly draftDay,
        DateOnly publishDay,
        SyntheticRecord version,
        IReadOnlyList<(string FieldPath, string? OldValue, string? NewValue)> changes
    )
    {
        double rejectionChance = draft.BasedOnRevision is null
            ? FirstSubmissionRejectionChance
            : UpdateRejectionChance;
        RecordRevision revision = draft;
        if (publishDay.DayNumber - draftDay.DayNumber >= 3 && _random.Double() < rejectionChance)
        {
            DateOnly submitted = draftDay.AddDays(1);
            Submit(revision, At(submitted, 10, 12));
            DateOnly rejected = submitted.AddDays(1);
            Reject(revision, At(rejected, 9, 11));
            AddContent(revision, version);
            revision = CreateRevision(At(rejected, 11, 13), basedOn: revision);
        }

        Edit(revision, At(publishDay, 11, 12));
        Submit(revision, At(publishDay, 12, 13));
        Approve(revision, At(publishDay, 13, 15), changes);
        AddContent(revision, version);
        return revision;
    }

    private RecordRevision CreateRevision(DateTime at, RecordRevision? basedOn)
    {
        RecordRevision revision = new()
        {
            Record = _record,
            BasedOnRevision = basedOn,
            WorkflowStatus = WorkflowStatus.Draft,
            CreatedAt = at,
            CreatedByUser = _author,
        };
        _record.Revisions.Add(revision);
        AddEvent(
            basedOn is null ? RecordEventType.RecordCreated : RecordEventType.RevisionCreated,
            at,
            _author,
            revision
        );
        return revision;
    }

    private void Edit(RecordRevision revision, DateTime at)
    {
        revision.UpdatedAt = at;
        revision.UpdatedByUser = _author;
    }

    private void Submit(RecordRevision revision, DateTime at)
    {
        revision.SubmittedAt = at;
        revision.SubmittedByUser = _author;
        revision.WorkflowStatus = WorkflowStatus.InReview;
        AddEvent(RecordEventType.SubmittedToQa, at, _author, revision);
    }

    private void Reject(RecordRevision revision, DateTime at)
    {
        string note = Pick(_rejectionNotes);
        QaReview review = Review(revision, at, QaOutcome.Rejected, note);
        revision.WorkflowStatus = WorkflowStatus.Rejected;
        AddEvent(
            RecordEventType.RevisionRejected,
            at,
            review.ReviewedByUser,
            revision,
            review,
            note
        );
    }

    private void Approve(
        RecordRevision revision,
        DateTime at,
        IReadOnlyList<(string FieldPath, string? OldValue, string? NewValue)> changes
    )
    {
        QaReview review = Review(revision, at, QaOutcome.Approved, note: null);
        revision.WorkflowStatus = WorkflowStatus.Published;
        RecordEvent published = AddEvent(
            RecordEventType.RecordPublished,
            at,
            review.ReviewedByUser,
            revision,
            review
        );
        foreach ((string fieldPath, string? oldValue, string? newValue) in changes)
        {
            published.FieldChanges.Add(
                new RecordEventFieldChange
                {
                    FieldPath = fieldPath,
                    OldValue = oldValue,
                    NewValue = newValue,
                }
            );
        }
        _record.ReviewedAt = at;
    }

    private QaReview Review(RecordRevision revision, DateTime at, QaOutcome outcome, string? note)
    {
        QaReview review = new()
        {
            Revision = revision,
            Outcome = outcome,
            Note = note,
            ReviewedByUser = Pick(_reviewers),
            ReviewedAt = at,
        };
        revision.QaReviews.Add(review);
        return review;
    }

    private void ReviewWithoutChange(DateTime at, RecordRevision publishedRevision)
    {
        AddEvent(RecordEventType.RecordReviewedNoChange, at, _author, publishedRevision);
        _record.ReviewedAt = at;
    }

    private void ChangeStatus(SyntheticStatusChange change, DateTime at)
    {
        // Automatic archiving is done by the system rather than a person, and is not a review.
        bool automatic = change.Reason == RecordStatusChangeReason.ArchivedAutomatically;
        User? changedBy = automatic ? null : _author;
        _record.ChangeStatus(_source.RecordStatus, at, changedBy, change.Reason, change.Note);
        AddEvent(
            RecordEventType.RecordStatusChanged,
            at,
            changedBy,
            revision: null,
            note: change.Note
        );
        if (!automatic)
        {
            _record.ReviewedAt = at;
        }
    }

    private RecordEvent AddEvent(
        RecordEventType eventType,
        DateTime at,
        User? performedBy,
        RecordRevision? revision,
        QaReview? review = null,
        string? note = null
    )
    {
        RecordEvent recordEvent = new()
        {
            Record = _record,
            Revision = revision,
            QaReview = review,
            EventType = eventType,
            PerformedAt = at,
            PerformedByUser = performedBy,
            Note = note,
        };
        _record.Events.Add(recordEvent);
        return recordEvent;
    }

    private void AddContent(RecordRevision revision, SyntheticRecord version) =>
        _content.AddRange(_contentFactory.Create(version, revision));

    /// <summary>A time during working hours on the given day, from the start of one hour to the start of another.</summary>
    private DateTime At(DateOnly day, int fromHour, int toHour) =>
        DateTime.SpecifyKind(
            day.ToDateTime(new TimeOnly(fromHour, 0))
                .AddMinutes(_random.Number(0, ((toHour - fromHour) * 60) - 1)),
            DateTimeKind.Utc
        );

    private DateOnly DayBetween(DateOnly first, DateOnly last) =>
        last <= first ? last : first.AddDays(_random.Number(0, last.DayNumber - first.DayNumber));

    private T Pick<T>(IReadOnlyList<T> values) => values[_random.Number(0, values.Count - 1)];
}
