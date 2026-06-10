using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Email;

namespace UKPS.Api.Configurations.Email;

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
