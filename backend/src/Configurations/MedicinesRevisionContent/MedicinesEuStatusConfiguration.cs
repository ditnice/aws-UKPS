using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesEuStatusConfiguration : IEntityTypeConfiguration<MedicinesEuStatus>
{
    public void Configure(EntityTypeBuilder<MedicinesEuStatus> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EuOrphanStatus).HasConversion<string>();
        builder.Property(x => x.EuAtmpClassificationStatus).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_eu_status_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EuOrphanGrantedDate)
               .WithMany()
               .HasForeignKey(x => x.EuOrphanGrantedDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AtmpClassification)
               .WithMany()
               .HasForeignKey(x => x.AtmpClassificationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
