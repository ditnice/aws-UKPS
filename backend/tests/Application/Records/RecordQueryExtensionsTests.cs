using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Records;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordQueryExtensionsTests : DatabaseTestBase
{
    public RecordQueryExtensionsTests(PostgresFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task SelectLatestRevision_OnHoldRecord_ReturnsDueDateAfterSixMonths()
    {
        var reviewedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var organisation = (
            await AddEntities(
                new OrganisationFaker().Generate(1),
                TestContext.Current.CancellationToken
            )
        ).Single();
        var record = new RecordFaker()
            .RuleFor(x => x.OrganisationId, _ => organisation.Id)
            .RuleFor(x => x.RecordStatus, _ => RecordStatus.OnHold)
            .RuleFor(x => x.ReviewedAt, _ => reviewedAt)
            .Generate();

        await AddEntities(new[] { record }, TestContext.Current.CancellationToken);
        var result = await Context
            .Records.Where(x => x.Id == record.Id)
            .SelectLatestRevision()
            .SingleAsync(TestContext.Current.CancellationToken);
        result.NextUpdateDue.ShouldBe(reviewedAt.AddMonths(6));
    }

    [Fact]
    public async Task SelectLatestRevision_PublishedVaccine_ReturnsDueDateAfterSixMonths()
    {
        var reviewedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var organisation = (
            await AddEntities(
                new OrganisationFaker().Generate(1),
                TestContext.Current.CancellationToken
            )
        ).Single();
        var record = new RecordFaker()
            .RuleFor(x => x.OrganisationId, _ => organisation.Id)
            .RuleFor(x => x.RecordType, _ => RecordType.Vaccine)
            .RuleFor(x => x.RecordStatus, _ => RecordStatus.Published)
            .RuleFor(x => x.ReviewedAt, _ => reviewedAt)
            .Generate();
        await AddEntities(new[] { record }, TestContext.Current.CancellationToken);
        var result = await Context
            .Records.Where(x => x.Id == record.Id)
            .SelectLatestRevision()
            .SingleAsync(TestContext.Current.CancellationToken);
        result.NextUpdateDue.ShouldBe(reviewedAt.AddMonths(6));
    }

    [Fact]
    public async Task SelectLatestRevision_PublishedMedicine_ReturnsDueDateAfterThreeMonths()
    {
        var reviewedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var organisation = (
            await AddEntities(
                new OrganisationFaker().Generate(1),
                TestContext.Current.CancellationToken
            )
        ).Single();
        var record = new RecordFaker()
            .RuleFor(x => x.OrganisationId, _ => organisation.Id)
            .RuleFor(x => x.RecordType, _ => RecordType.Medicine)
            .RuleFor(x => x.RecordStatus, _ => RecordStatus.Published)
            .RuleFor(x => x.ReviewedAt, _ => reviewedAt)
            .Generate();
        await AddEntities(new[] { record }, TestContext.Current.CancellationToken);
        var result = await Context
            .Records.Where(x => x.Id == record.Id)
            .SelectLatestRevision()
            .SingleAsync(TestContext.Current.CancellationToken);
        result.NextUpdateDue.ShouldBe(reviewedAt.AddMonths(3));
    }
}
