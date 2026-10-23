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
    private const double ProvisionalValueChance = 0.5;

    private readonly SyntheticRecord _source;
    private readonly IReadOnlyList<User> _authors;
    private readonly IReadOnlyList<User> _reviewers;
    private readonly RecordContentFactory _contentFactory;
    private readonly DateOnly _today;
    private readonly Randomizer _random;
    private readonly List<object> _content = [];
    private readonly List<RecordFieldAudit> _audits = [];

    /// <summary>Each audited field's value as of the latest save, to any revision.</summary>
    private readonly Dictionary<string, string?> _saved = new(StringComparer.Ordinal);
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

        return new SeededRecord(_record, _content, _audits);
    }

    private void BuildDraftHistory(RecordRevision first)
    {
        DateOnly lastUpdated = _source.LastUpdatedAt;
        double outcome = _random.Double();
        bool canBeRejected = lastUpdated.DayNumber - _source.CreatedAt.DayNumber >= 2;

        if (outcome < 0.2 && canBeRejected && PickSubmissionFlaw(_source) is { } flaw)
        {
            // Submitted once, sent back by QA, and now being reworked.
            DateOnly submitted = _source.CreatedAt.AddDays(1);
            SaveDraftEdits(first, At(submitted, 9, 10), flaw.Submitted);
            Submit(first, At(submitted, 10, 12));
            DateOnly rejected = submitted.AddDays(1);
            Reject(first, At(rejected, 9, 11), flaw.Note);

            RecordRevision rework = CreateRevision(At(rejected, 11, 13), basedOn: first);
            SaveDraftEdits(rework, At(lastUpdated, 14, 17), _source);
            return;
        }

        SaveDraftEdits(first, At(lastUpdated, 12, 15), _source);
        if (outcome < 0.45)
        {
            Submit(first, At(lastUpdated, 15, 17));
        }
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
                publishedRevision = Publish(first, _source.CreatedAt, publishDays[index], version);
                QaReview approval = publishedRevision.QaReviews.Last();
                _record.ChangeStatus(
                    RecordStatus.Published,
                    approval.ReviewedAt!.Value,
                    approval.ReviewedByUser
                );
                published = version;
                continue;
            }

            if (!RecordContentHistory.HasChanges(published, version))
            {
                // Before any status change made later the same day.
                ConfirmAsCurrent(At(publishDays[index], 10, 15), publishedRevision);
                continue;
            }

            if (_random.Double() < 0.3)
            {
                _author = Pick(_authors);
            }
            DateOnly draftDay = DayBetween(publishDays[index - 1].AddDays(1), publishDays[index]);
            RecordRevision draft = CreateRevision(At(draftDay, 9, 11), basedOn: publishedRevision);
            publishedRevision = Publish(draft, draftDay, publishDays[index], version);
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
            ConfirmAsCurrent(At(reviewed, 10, 16), publishedRevision);
        }

        DateOnly lastActivity = DateOnly.FromDateTime(_record.ReviewedAt!.Value);
        bool hasTimeForDraft = _today.DayNumber - lastActivity.DayNumber >= 7;
        if (
            _source.RecordStatus == RecordStatus.Published
            && hasTimeForDraft
            && _random.Double() < PendingDraftChance
        )
        {
            // A further update is being prepared but has not been published yet.
            DateOnly draftDay = DayBetween(lastActivity.AddDays(1), _today.AddDays(-1));
            RecordRevision draft = CreateRevision(At(draftDay, 9, 11), basedOn: publishedRevision);
            // Records without estimated dates to push back have a draft opened but not yet changed.
            SyntheticRecord? update = RecordContentHistory.WithEstimatedUkDatesPushedBack(_source);
            SaveDraftEdits(draft, At(draftDay, 11, 14), update ?? _source);
            if (update is not null && _random.Double() < 0.3)
            {
                Submit(draft, At(draftDay, 14, 17));
            }
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

        IReadOnlyDictionary<string, string?> fields = RecordContentHistory.AuditedFields(_source);
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
        SyntheticRecord version
    )
    {
        double rejectionChance = draft.BasedOnRevision is null
            ? FirstSubmissionRejectionChance
            : UpdateRejectionChance;
        RecordRevision revision = draft;
        if (
            publishDay.DayNumber - draftDay.DayNumber >= 3
            && _random.Double() < rejectionChance
            && PickSubmissionFlaw(version) is { } flaw
        )
        {
            DateOnly submitted = draftDay.AddDays(1);
            SaveDraftEdits(revision, At(submitted, 9, 10), flaw.Submitted);
            Submit(revision, At(submitted, 10, 12));
            DateOnly rejected = submitted.AddDays(1);
            Reject(revision, At(rejected, 9, 11), flaw.Note);
            revision = CreateRevision(At(rejected, 11, 13), basedOn: revision);
        }

        SaveDraftEdits(revision, At(publishDay, 11, 12), version);
        DateTime resubmitted = At(publishDay, 12, 13);
        Submit(revision, resubmitted);
        Approve(revision, At(publishDay, 13, 15), resubmitted);
        return revision;
    }

    /// <summary>Picks a mistake QA could send back for <paramref name="version"/>, if any apply.</summary>
    private (string Note, SyntheticRecord Submitted)? PickSubmissionFlaw(SyntheticRecord version)
    {
        (string Note, SyntheticRecord Submitted)[] applicable =
        [
            .. RecordContentHistory
                .SubmissionFlaws.Select(flaw => (flaw.Note, Submitted: flaw.Introduce(version)))
                .Where(flaw => flaw.Submitted is not null)
                .Select(flaw => (flaw.Note, flaw.Submitted!)),
        ];
        return applicable.Length == 0 ? null : Pick(applicable);
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

    /// <summary>
    /// Saves edits to a revision one or more times between its creation and
    /// <paramref name="lastSave"/>, leaving it with the content of <paramref name="version"/>.
    /// Some fields are saved with a provisional value first.
    /// </summary>
    private void SaveDraftEdits(RecordRevision revision, DateTime lastSave, SyntheticRecord version)
    {
        AddContent(revision, version);
        IReadOnlyDictionary<string, string?> target = RecordContentHistory.AuditedFields(version);
        string[] changed = [.. target.Keys.Where(field => !IsSaved(field, target[field]))];
        if (changed.Length == 0)
        {
            return;
        }

        // A new record is filled in from scratch a section at a time, so it takes more saves
        // than a later revision that only changes a few fields.
        bool isNewRecord = _saved.Count == 0;
        int plannedSaves = isNewRecord ? _random.Number(2, 4) : _random.Number(1, 3);
        DateTime[] times = SaveTimes(revision.CreatedAt, lastSave, plannedSaves);
        int saves = Math.Min(times.Length, changed.Length);
        times = times[^saves..];

        HashSet<string> provisionallyUnknown =
        [
            .. RecordContentHistory.DisclosableFields.Where(_ => _random.Bool()),
        ];
        IReadOnlyDictionary<string, string?> provisional = RecordContentHistory.AuditedFields(
            RecordContentHistory.AsKnownAt(
                version,
                DateOnly.FromDateTime(revision.CreatedAt),
                _random.Number(1, 3),
                provisionallyUnknown
            )
        );

        for (int save = 0; save < saves - 1; save++)
        {
            IEnumerable<string> section = changed
                .Skip(save * changed.Length / saves)
                .Take(((save + 1) * changed.Length / saves) - (save * changed.Length / saves));
            Save(
                revision,
                times[save],
                section.Select(field =>
                {
                    string? value = target[field];
                    bool usesProvisional =
                        provisional[field] is { } guess
                        && !string.Equals(guess, value, StringComparison.Ordinal)
                        && _random.Double() < ProvisionalValueChance;
                    return (field, usesProvisional ? provisional[field] : value);
                })
            );
        }
        Save(revision, times[^1], changed.Select(field => (field, target[field])));
    }

    /// <summary>
    /// Up to <paramref name="count"/> distinct save times during working hours after
    /// <paramref name="start"/>, ending with <paramref name="end"/>.
    /// </summary>
    private DateTime[] SaveTimes(DateTime start, DateTime end, int count)
    {
        SortedSet<DateTime> times = [end];
        DateOnly firstDay = DateOnly.FromDateTime(start);
        DateOnly lastDay = DateOnly.FromDateTime(end);
        for (int attempt = 1; attempt < count; attempt++)
        {
            DateTime time = At(DayBetween(firstDay, lastDay), 9, 18);
            if (time > start && time < end)
            {
                times.Add(time);
            }
        }
        return [.. times];
    }

    private void Save(
        RecordRevision revision,
        DateTime at,
        IEnumerable<(string Field, string? Value)> values
    )
    {
        foreach ((string field, string? value) in values)
        {
            if (IsSaved(field, value))
            {
                continue;
            }

            _audits.Add(
                new RecordFieldAudit
                {
                    Record = _record,
                    Revision = revision,
                    FieldPath = field,
                    OldValue = _saved.GetValueOrDefault(field),
                    NewValue = value,
                    ChangedByUser = _author,
                    ChangedAt = at,
                }
            );
            _saved[field] = value;
        }
    }

    private bool IsSaved(string field, string? value) =>
        string.Equals(_saved.GetValueOrDefault(field), value, StringComparison.Ordinal);

    private void Submit(RecordRevision revision, DateTime at)
    {
        revision.WorkflowStatus = WorkflowStatus.QAReview;
        AddEvent(RecordEventType.SubmittedToQa, at, _author, revision);
    }

    private void Reject(RecordRevision revision, DateTime at, string note)
    {
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

    private void Approve(RecordRevision revision, DateTime at, DateTime submittedAt)
    {
        QaReview review = Review(revision, at, QaOutcome.Approved, note: null);
        revision.WorkflowStatus = WorkflowStatus.Published;
        AddEvent(RecordEventType.RecordPublished, at, review.ReviewedByUser, revision, review);
        // The organisation confirmed the record was current when they submitted it.
        _record.ReviewedAt = submittedAt;
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

    private void ConfirmAsCurrent(DateTime at, RecordRevision publishedRevision)
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

    private void AddEvent(
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
