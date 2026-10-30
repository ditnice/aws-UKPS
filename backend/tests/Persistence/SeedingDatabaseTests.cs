using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Configurations.RecordWorkflow;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class SeedingDatabaseTests : DatabaseTestBase
{
    public SeedingDatabaseTests(PostgresFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task SeededData_ShouldSaveToTheDatabaseAndBeReadableByTheRecordServices()
    {
        var ct = TestContext.Current.CancellationToken;
        SeedingDataPayload payload = DataSeederInMemory.BuildPayload(new SeedingOptions());

        await payload.SaveAsync(Context, ct);
        Context.ChangeTracker.Clear();

        (
            await Context
                .Records.IgnoreQueryFilters([RecordConfiguration.DiscardedFilter])
                .CountAsync(ct)
        ).ShouldBe(566);
        Organisation first = await Context.Organisations.SingleAsync(o => o.Id == 1, ct);
        first.OrganisationType.ShouldBe(OrganisationType.Internal);

        foreach (
            IGrouping<int, Record> organisationRecords in payload.Records.GroupBy(r =>
                r.OrganisationId
            )
        )
        {
            ServiceTestHarness<IRecordViewService> viewHarness = new(Context);
            viewHarness.UpdateCurrentUser(x =>
                x with
                {
                    OrganisationId = organisationRecords.Key,
                    UserRole = UserRole.Standard,
                }
            );
            await ShouldListEveryRecord(organisationRecords, ct);
            await ShouldShowEveryRecord(
                viewHarness.Service,
                organisationRecords.Where(r => r.RecordStatus != RecordStatus.Discarded),
                ct
            );
            await ShouldHideDiscardedRecords(
                viewHarness.Service,
                organisationRecords.Where(r => r.RecordStatus == RecordStatus.Discarded),
                ct
            );
        }

        await ShouldAuditReferenceAnswersAsTheStoredIds(ct);
    }

    /// <summary>
    /// The latest audit of each indication reference answer holds what the record's latest
    /// revision stores, as the form would read it.
    /// </summary>
    private async Task ShouldAuditReferenceAnswersAsTheStoredIds(CancellationToken ct)
    {
        Dictionary<(int RecordId, string FieldPath), string?> latestAudits = (
            await Context
                .RecordFieldAudits.Where(a =>
                    a.FieldPath == "medicines_indication_detail.bnf_chapter_id"
                    || a.FieldPath == "medicines_indication_detail_therapeutic_area"
                )
                .ToArrayAsync(ct)
        )
            .GroupBy(a => (a.RecordId, a.FieldPath))
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(a => a.ChangedAt).ThenBy(a => a.Id).Last().NewValue
            );
        latestAudits.ShouldNotBeEmpty();

        var latestRows = await Context
            .MedicinesIndicationDetails.Where(d =>
                d.RevisionId
                == Context
                    .RecordRevisions.Where(r => r.RecordId == d.Revision!.RecordId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ThenByDescending(r => r.Id)
                    .Select(r => r.Id)
                    .First()
            )
            .Select(d => new
            {
                d.Revision!.RecordId,
                d.BnfChapterId,
                TherapeuticAreaIds = d.TherapeuticAreas.Select(t => t.TherapeuticAreaId).ToArray(),
            })
            .ToArrayAsync(ct);

        foreach (var row in latestRows)
        {
            latestAudits
                .GetValueOrDefault((row.RecordId, "medicines_indication_detail.bnf_chapter_id"))
                .ShouldBe(row.BnfChapterId?.ToString(CultureInfo.InvariantCulture));
            latestAudits
                .GetValueOrDefault((row.RecordId, "medicines_indication_detail_therapeutic_area"))
                .ShouldBe(
                    row.TherapeuticAreaIds.Length == 0
                        ? null
                        : JsonSerializer.Serialize(
                            row.TherapeuticAreaIds.Select(id =>
                                    id.ToString(CultureInfo.InvariantCulture)
                                )
                                .Order(StringComparer.Ordinal)
                        )
                );
        }
    }

    private async Task ShouldListEveryRecord(
        IGrouping<int, Record> organisationRecords,
        CancellationToken ct
    )
    {
        ServiceTestHarness<IRecordService> listHarness = new(Context);
        listHarness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = organisationRecords.Key,
                UserRole = UserRole.Standard,
            }
        );

        Result<PaginatedResponseDto<RecordListItemDto>, GetRecordsError> result =
            await listHarness.Service.GetOrganisationRecords(
                organisationRecords.Key,
                new GetRecordsQueryDto(),
                ct
            );

        result
            .ShouldBeSuccess()
            .TotalCount.ShouldBe(
                organisationRecords.Count(r => r.RecordStatus != RecordStatus.Discarded)
            );
    }

    private static async Task ShouldHideDiscardedRecords(
        IRecordViewService service,
        IEnumerable<Record> records,
        CancellationToken ct
    )
    {
        foreach (Record record in records)
        {
            Result<RecordDto, GetRecordError> result = await service.GetRecord(
                record.Id,
                RecordType.Medicine,
                record.OrganisationId,
                ct
            );

            result.ShouldBeError().ShouldBeOfType<GetRecordError.NotFound>();
        }
    }

    private static async Task ShouldShowEveryRecord(
        IRecordViewService service,
        IEnumerable<Record> records,
        CancellationToken ct
    )
    {
        foreach (Record record in records)
        {
            Result<RecordDto, GetRecordError> result = await service.GetRecord(
                record.Id,
                RecordType.Medicine,
                record.OrganisationId,
                ct
            );

            var dto = result.ShouldBeSuccess().ShouldBeOfType<MedicineRecordDto>();
            dto.RecordProductDetail.ShouldNotBeNull();
            dto.MedicinesIndicationDetail.ShouldNotBeNull().TherapeuticAreas.ShouldNotBeEmpty();
        }
    }
}
