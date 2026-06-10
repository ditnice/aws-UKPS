using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.Reporting;
using UKPS.Data.Entities.Email;
using UKPS.Data.Entities.UserFeatures;

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

namespace UKPS.Data.Configurations.Email;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("email_template", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Title).IsRequired();
    }
}

public class EmailAuditConfiguration : IEntityTypeConfiguration<EmailAudit>
{
    public void Configure(EntityTypeBuilder<EmailAudit> builder)
    {
        builder.ToTable("email_audit", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Recipients).IsRequired();
        builder.Property(x => x.SentAt).HasColumnType("timestamptz").IsRequired();

        builder.HasOne(x => x.Template)
               .WithMany(x => x.EmailAudits)
               .HasForeignKey(x => x.TemplateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

namespace UKPS.Data.Configurations.UserFeatures;

public class RecordWatchlistConfiguration : IEntityTypeConfiguration<RecordWatchlist>
{
    public void Configure(EntityTypeBuilder<RecordWatchlist> builder)
    {
        builder.ToTable("record_watchlist", "ukps");
        builder.HasKey(x => new { x.UserId, x.RecordId });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Record)
               .WithMany()
               .HasForeignKey(x => x.RecordId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
