using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Tests.Utilities.Fixtures;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ClinicalTrialDisplayOrderMigrationTests(PostgresFixture fixture)
    : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task Up_BackfillsEachRevisionsTrialsInIdOrder()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        Organisation organisation = await AddEntity(new OrganisationFaker().Generate(), ct);
        User user = await AddEntity(new UserFaker().Generate(), ct);
        Record record = await AddEntity(
            new RecordFaker().RuleFor(x => x.OrganisationId, organisation.Id).Generate(),
            ct
        );
        RecordRevision[] revisions =
        [
            .. await AddEntities(
                new RecordRevisionFaker()
                    .RuleFor(x => x.RecordId, record.Id)
                    .RuleFor(x => x.CreatedBy, user.Id)
                    .Generate(2),
                ct
            ),
        ];
        Context.AddRange(
            revisions.Select(r => new RecordClinicalTrialInformation { RevisionId = r.Id })
        );
        await Context.SaveChangesAsync(ct);

        // Interleave revisions so a global row number would not restart from 1 for each one.
        for (int i = 0; i < 4; i++)
        {
            await AddEntity(
                new RecordClinicalTrial
                {
                    RevisionId = revisions[i % revisions.Length].Id,
                    StudyName = $"Study {i}",
                    ClinicalTrialsGovNumber = $"NCT{i:D8}",
                },
                ct
            );
        }
        Context.ChangeTracker.Clear();

        IMigrator migrator = Context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261117163237_AddClinicalTrialsForm", ct);
        try
        {
            await migrator.MigrateAsync(cancellationToken: ct);

            RecordClinicalTrial[] trials = await Context
                .RecordClinicalTrials.AsNoTracking()
                .OrderBy(static t => t.Id)
                .ToArrayAsync(ct);
            foreach (RecordRevision revision in revisions)
            {
                trials
                    .Where(t => t.RevisionId == revision.Id)
                    .Select(static t => t.DisplayOrder)
                    .ShouldBe([1, 2]);
            }
        }
        finally
        {
            // Leave the shared test database at the latest schema even if an assertion fails.
            await migrator.MigrateAsync(cancellationToken: ct);
        }
    }
}
