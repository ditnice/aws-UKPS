using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
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

        Context.AddRange(payload.GetAllEntities());
        await Context.SaveChangesAsync(ct);
        Context.ChangeTracker.Clear();

        (await Context.Records.CountAsync(ct)).ShouldBe(500);
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
            await ShouldShowEveryRecord(viewHarness.Service, organisationRecords, ct);
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

        result.ShouldBeSuccess().TotalCount.ShouldBe(organisationRecords.Count());
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
                ct
            );

            var dto = result.ShouldBeSuccess().ShouldBeOfType<MedicineRecordDto>();
            dto.RevisionId.ShouldBe(
                record.Revisions.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id).Last().Id
            );
            dto.RecordProductDetail.ShouldNotBeNull();
            dto.MedicinesIndicationDetail.ShouldNotBeNull().TherapeuticAreas.ShouldNotBeEmpty();
        }
    }
}
