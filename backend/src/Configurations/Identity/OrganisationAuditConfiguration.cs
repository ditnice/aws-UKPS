using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

internal sealed class OrganisationAuditConfiguration : IEntityTypeConfiguration<OrganisationAudit>
{
    public void Configure(EntityTypeBuilder<OrganisationAudit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EventType).HasConversion<string>();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.OrganisationId)
               .HasDatabaseName("ix_organisation_audit_organisation_id");

        builder.HasOne(x => x.Organisation)
               .WithMany(x => x.OrganisationAudits)
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
               .WithMany()
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
