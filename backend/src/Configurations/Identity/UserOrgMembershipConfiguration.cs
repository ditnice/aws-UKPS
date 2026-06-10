using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.Identity;
using UKPS.Data.Enums;

namespace UKPS.Data.Configurations.Identity;

public class UserOrgMembershipConfiguration : IEntityTypeConfiguration<UserOrgMembership>
{
    public void Configure(EntityTypeBuilder<UserOrgMembership> builder)
    {
        builder.ToTable("user_org_membership", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.UserRole).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();
        // PharmaceuticalEntity is a [Flags] integer
        builder.Property(x => x.AllowedPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        builder.HasIndex(
                   x => new { x.UserId, x.OrganisationId, x.AllowedPharmaceuticalEntity })
               .IsUnique()
               .HasDatabaseName("ix_user_org_membership_user_org_entity");

        builder.HasIndex(x => x.OrganisationId)
               .HasDatabaseName("ix_user_org_membership_organisation_id");

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserOrgMemberships)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organisation)
               .WithMany(x => x.UserOrgMemberships)
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
