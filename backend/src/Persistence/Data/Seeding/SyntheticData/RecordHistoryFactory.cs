using System.Globalization;
using Bogus;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// Turns synthetic records into records with a plausible workflow history: drafts, QA reviews,
/// publications, reviews and status changes.
/// </summary>
internal sealed class RecordHistoryFactory
{
    private readonly RecordContentFactory _contentFactory;
    private readonly DateOnly _today;

    public RecordHistoryFactory(RecordContentFactory contentFactory, DateOnly today)
    {
        _contentFactory = contentFactory;
        _today = today;
    }

    /// <summary>Builds a record and its history from its synthetic content.</summary>
    /// <param name="source">The record's latest content and lifecycle dates.</param>
    /// <param name="organisation">The organisation that owns the record.</param>
    /// <param name="authors">Members of the organisation who edit the record.</param>
    /// <param name="reviewers">QA users who review submitted revisions.</param>
    public SeededRecord Create(
        SyntheticRecord source,
        Organisation organisation,
        IReadOnlyList<User> authors,
        IReadOnlyList<User> reviewers
    )
    {
        // Seeding each record separately keeps its history stable when other records change.
        Randomizer random = new(int.Parse(source.SourceId, CultureInfo.InvariantCulture));
        return new RecordHistoryBuilder(
            source,
            organisation,
            authors,
            reviewers,
            _contentFactory,
            _today,
            random
        ).Build();
    }
}
