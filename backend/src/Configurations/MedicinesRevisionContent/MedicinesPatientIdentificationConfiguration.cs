using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesPatientIdentificationConfiguration
    : IEntityTypeConfiguration<MedicinesPatientIdentification>
{
    public void Configure(EntityTypeBuilder<MedicinesPatientIdentification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ScreeningRequired).HasConversion<string>();
        builder.Property(x => x.UrgentIdentificationRequired).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_patient_identification_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
