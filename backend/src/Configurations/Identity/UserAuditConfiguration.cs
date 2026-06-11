using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

public class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
{
    public void Configure(EntityTypeBuilder<UserAudit> builder)
    {
        builder.ToTable("user_audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EventType).HasConversion<string>();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("ix_user_audit_user_id");

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserAudits)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
               .WithMany()
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
