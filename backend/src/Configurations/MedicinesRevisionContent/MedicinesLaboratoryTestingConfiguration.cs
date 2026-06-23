using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesLaboratoryTestingConfiguration
    : IEntityTypeConfiguration<MedicinesLaboratoryTesting>
{
    public void Configure(EntityTypeBuilder<MedicinesLaboratoryTesting> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DiagnosticTestRequired);
        builder.Property(x => x.GenomicTestRequired);
        builder.Property(x => x.GenomicTestInNationalDirectory);
        builder.Property(x => x.GenomicTurnaroundConsiderations);
        builder.Property(x => x.GenomicTestMandatory);
        builder.Property(x => x.MonitoringTestsRequired);

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_laboratory_testing_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GenomicSampleType)
               .WithMany()
               .HasForeignKey(x => x.GenomicSampleTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PatientPathwayPoint)
               .WithMany()
               .HasForeignKey(x => x.PatientPathwayPointId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
