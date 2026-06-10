using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.Identity;
using UKPS.Data.Enums;

namespace UKPS.Data.Configurations.Identity;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisation", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.OrganisationName).IsRequired();
        builder.Property(x => x.OrganisationType).HasConversion<string>();
        // PharmaceuticalEntity is a [Flags] integer — stored as int, not a PG enum
        builder.Property(x => x.AllowedPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");

    }
}

public class TermsAcceptanceConfiguration : IEntityTypeConfiguration<TermsAcceptance>
{
    public void Configure(EntityTypeBuilder<TermsAcceptance> builder)
    {
        builder.ToTable("terms_acceptance", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RelevantPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.SignatoryName).IsRequired();
        builder.Property(x => x.SignatoryEmail).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.LinkExpiresAt).HasColumnType("timestamptz");
        builder.Property(x => x.SignedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Organisation)
               .WithMany(x => x.TermsAcceptances)
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrganisationAuditConfiguration : IEntityTypeConfiguration<OrganisationAudit>
{
    public void Configure(EntityTypeBuilder<OrganisationAudit> builder)
    {
        builder.ToTable("organisation_audit", "ukps");
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

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table is named app_user to avoid collision with PostgreSQL reserved word 'user'
        builder.ToTable("app_user", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Username).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique()
               .HasDatabaseName("ix_app_user_username");
        builder.Property(x => x.UserType).HasConversion<string>();
        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();
        builder.Property(x => x.LoginTime).HasColumnType("timestamptz");
        builder.Property(x => x.LogoutTime).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
    }
}

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

public class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
{
    public void Configure(EntityTypeBuilder<UserAudit> builder)
    {
        builder.ToTable("user_audit", "ukps");
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
