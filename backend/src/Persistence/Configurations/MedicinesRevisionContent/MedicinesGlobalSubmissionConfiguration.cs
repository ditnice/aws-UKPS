using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesGlobalSubmissionConfiguration
    : IEntityTypeConfiguration<MedicinesGlobalSubmission>
{
    public void Configure(EntityTypeBuilder<MedicinesGlobalSubmission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_global_submission_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.GlobalSubmissionActualDate)
            .WithMany()
            .HasForeignKey(x => x.GlobalSubmissionActualDateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
