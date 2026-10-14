using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Utilities.Data;

/// <summary>
/// Creates a medicine record the way record creation does: a draft first revision with its
/// <see cref="RecordProductDetail"/> row. Record creation does not insert a
/// <see cref="MedicinesIndicationDetail"/> row (the form creates it on first save); pass
/// <c>withIndicationDetail</c> to start with an empty one.
/// </summary>
internal static class MedicineRecordTestData
{
    public static async Task<MedicineRecord> CreateAsync(
        AppDbContext context,
        CancellationToken cancellationToken,
        Organisation? organisation = null,
        User? user = null,
        bool withIndicationDetail = false
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
        var productDetail = new RecordProductDetail
        {
            CompanyCode = "ABC-123",
            RecordTitle = "Test record",
            Revision = revision,
        };
        context.Add(productDetail);
        var indicationDetail = withIndicationDetail
            ? new MedicinesIndicationDetail { Revision = revision }
            : null;
        if (indicationDetail is not null)
        {
            context.Add(indicationDetail);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new MedicineRecord(
            record,
            revision,
            productDetail,
            indicationDetail,
            organisation,
            user
        );
    }
}
