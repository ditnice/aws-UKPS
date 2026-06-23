using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesEamsPimConfiguration : IEntityTypeConfiguration<MedicinesEamsPim>
{
    public void Configure(EntityTypeBuilder<MedicinesEamsPim> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.PimDesignationStatus);
        builder.Property(x => x.WillSubmitToEams);
        builder.Property(x => x.EamsOpinionDecision);

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_eams_pim_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EamsSubmissionDate)
               .WithMany()
               .HasForeignKey(x => x.EamsSubmissionDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EamsOpinionDate)
               .WithMany()
               .HasForeignKey(x => x.EamsOpinionDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
