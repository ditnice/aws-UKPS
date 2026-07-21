using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
{
    public void Configure(EntityTypeBuilder<UserAudit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EventType);
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_user_audit_user_id");

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.UserAudits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
