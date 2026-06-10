using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

public class MedicinesLaboratoryTestingConfiguration
    : IEntityTypeConfiguration<MedicinesLaboratoryTesting>
{
    public void Configure(EntityTypeBuilder<MedicinesLaboratoryTesting> builder)
    {
        builder.ToTable("medicines_laboratory_testing", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DiagnosticTestRequired).HasConversion<string>();
        builder.Property(x => x.GenomicTestRequired).HasConversion<string>();
        builder.Property(x => x.GenomicTestInNationalDirectory).HasConversion<string>();
        builder.Property(x => x.GenomicTurnaroundConsiderations).HasConversion<string>();
        builder.Property(x => x.GenomicTestMandatory).HasConversion<string>();
        builder.Property(x => x.MonitoringTestsRequired).HasConversion<string>();

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
