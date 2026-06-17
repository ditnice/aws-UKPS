using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UKPS.Api.Entities.Reporting;

namespace UKPS.Api.Configurations.Reporting;

internal sealed class ReportAuditConfiguration : IEntityTypeConfiguration<ReportAudit>
{
    public void Configure(EntityTypeBuilder<ReportAudit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        ValueConverter<JsonDocument?, string?> jsonConverter = new(
            doc => doc == null ? null : doc.RootElement.GetRawText(),
            json => json == null ? null : JsonDocument.Parse(json));

        builder.Property(x => x.Configuration).HasColumnType("jsonb").HasConversion(jsonConverter);
        builder.Property(x => x.FieldUsage).HasColumnType("jsonb").HasConversion(jsonConverter);
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
