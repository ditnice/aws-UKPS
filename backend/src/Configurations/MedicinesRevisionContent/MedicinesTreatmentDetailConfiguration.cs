using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

public class MedicinesTreatmentDetailConfiguration : IEntityTypeConfiguration<MedicinesTreatmentDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesTreatmentDetail> builder)
    {
        builder.ToTable("medicines_treatment_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ProposedPlaceInTherapy).IsRequired();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_treatment_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
