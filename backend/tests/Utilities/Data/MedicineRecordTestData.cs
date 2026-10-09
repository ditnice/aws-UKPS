using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Utilities.Data;

/// <summary>
/// Creates a medicine record the way record creation does: a draft first revision with its
/// <see cref="MedicinesProductDetail"/> row.
/// </summary>
internal static class MedicineRecordTestData
{
    public static async Task<MedicineRecord> CreateAsync(
        AppDbContext context,
        CancellationToken cancellationToken,
        Organisation? organisation = null,
        User? user = null
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        organisation ??= new OrganisationFaker()
            .RuleFor(x => x.Status, UserOrgStatus.Active)
            .Generate();
        user ??= new UserFaker().Generate();
        if (organisation.Id == 0)
        {
            context.Add(organisation);
        }

        if (user.Id == 0)
        {
            context.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);

        var (record, revision) = Record.CreateInitial(organisation, DateTime.UtcNow, user);
        var productDetail = new MedicinesProductDetail
        {
            RecordTitle = "Test record",
            Revision = revision,
        };
        context.Add(productDetail);
        await context.SaveChangesAsync(cancellationToken);

        return new MedicineRecord(record, revision, productDetail, organisation, user);
    }
}
