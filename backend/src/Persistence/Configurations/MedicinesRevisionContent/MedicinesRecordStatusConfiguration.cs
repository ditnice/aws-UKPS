using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.figurations.figurations.figurations.figurations.figurations.figurations.MedicinesRevisionContent;

internal sealed class MedicinesRecordStatusConfiguration
    : IEntityTypeConfiguration<MedicinesRecordStatus>
{
    public void Configure(EntityTypeBuilder<MedicinesRecordStatus> builder)
    {
        builder.HasKey(x => new { x.MedicinesProductDetailId, x.MedicineStatusTypeId });

        builder
            .HasOne(x => x.MedicinesProductDetail)
            .WithMany(x => x.RecordStatuses)
            .HasForeignKey(x => x.MedicinesProductDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.MedicineStatusType)
            .WithMany()
            .HasForeignKey(x => x.MedicineStatusTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
