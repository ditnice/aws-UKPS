using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

internal sealed class UserOrgMembershipConfiguration : IEntityTypeConfiguration<UserOrgMembership>
{
    public void Configure(EntityTypeBuilder<UserOrgMembership> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.UserRole);
        builder.Property(x => x.Status);
        // PharmaceuticalEntity is a [Flags] integer
        builder.Property(x => x.AllowedPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        builder
            .HasIndex(x => new
            {
                x.UserId,
                x.OrganisationId,
                x.AllowedPharmaceuticalEntity,
            })
            .IsUnique()
            .HasDatabaseName("ix_user_org_membership_user_org_entity");

        builder
            .HasIndex(x => x.OrganisationId)
            .HasDatabaseName("ix_user_org_membership_organisation_id");

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.UserOrgMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Organisation)
            .WithMany(x => x.UserOrgMemberships)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
