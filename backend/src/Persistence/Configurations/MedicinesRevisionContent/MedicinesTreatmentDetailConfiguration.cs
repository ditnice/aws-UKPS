using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.figurations.figurations.figurations.MedicinesRevisionContent;

internal sealed class MedicinesTreatmentDetailConfiguration
    : IEntityTypeConfiguration<MedicinesTreatmentDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesTreatmentDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ProposedPlaceInTherapy).IsRequired();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_treatment_detail_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
