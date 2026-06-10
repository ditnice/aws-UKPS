using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Reporting;

namespace UKPS.Api.Configurations.Reporting;

public class ReportAuditConfiguration : IEntityTypeConfiguration<ReportAudit>
{
    public void Configure(EntityTypeBuilder<ReportAudit> builder)
    {
        builder.ToTable("report_audit", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.Property(x => x.FieldUsage).HasColumnType("jsonb");
        builder.Property(x => x.RanAt).HasColumnType("timestamptz").IsRequired();

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReportPreset)
               .WithMany(x => x.ReportAudits)
               .HasForeignKey(x => x.ReportPresetId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
