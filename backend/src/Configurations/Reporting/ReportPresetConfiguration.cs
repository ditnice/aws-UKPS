using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.Reporting;

namespace UKPS.Data.Configurations.Reporting;

public class ReportPresetConfiguration : IEntityTypeConfiguration<ReportPreset>
{
    public void Configure(EntityTypeBuilder<ReportPreset> builder)
    {
        builder.ToTable("report_preset", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ApplicableUserType).HasConversion<string>();
        // PharmaceuticalEntity is a [Flags] integer
        builder.Property(x => x.ApplicablePharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Configuration)
               .HasColumnType("jsonb")
               .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
