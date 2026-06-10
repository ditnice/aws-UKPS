using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.MedicinesRevisionContent;

namespace UKPS.Data.Configurations.MedicinesRevisionContent;

public class MedicinesRecordStatusConfiguration : IEntityTypeConfiguration<MedicinesRecordStatus>
{
    public void Configure(EntityTypeBuilder<MedicinesRecordStatus> builder)
    {
        builder.ToTable("medicines_record_status", "ukps");
        builder.HasKey(x => new { x.MedicinesProductDetailId, x.MedicineStatusTypeId });

        builder.HasOne(x => x.MedicinesProductDetail)
               .WithMany(x => x.RecordStatuses)
               .HasForeignKey(x => x.MedicinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MedicineStatusType)
               .WithMany()
               .HasForeignKey(x => x.MedicineStatusTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
