using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table is named app_user to avoid collision with PostgreSQL reserved word 'user'
        builder.ToTable("app_user");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.CognitoUsername).HasMaxLength(39).HasColumnType("varchar(39)");
        builder
            .Property(x => x.CognitoUsername)
            .HasConversion((x) => x.Value, (x) => CognitoUsername.Parse(x));
        builder.Property(x => x.WorkEmail).IsRequired();
        builder
            .Property(u => u.NormalisedWorkEmail)
            .HasComputedColumnSql("ukps.normalise_email(work_email)", stored: true);
        builder
            .HasIndex(u => u.NormalisedWorkEmail)
            .IsUnique()
            .HasDatabaseName("ix_app_user_normalised_work_email");

        builder
            .Property(u => u.NormalisedPendingWorkEmail)
            .HasComputedColumnSql("ukps.normalise_email(pending_work_email)", stored: true);

        // Not unique: a timed-out pending email no longer reserves its address, and an index
        // predicate cannot depend on the current time. Uniqueness among active pending emails
        // is enforced by the application (see PendingEmailChange.HasActivePendingEmail).
        builder
            .HasIndex(u => u.NormalisedPendingWorkEmail)
            .HasFilter("normalised_pending_work_email IS NOT NULL")
            .HasDatabaseName("ix_app_user_normalised_pending_work_email");

        User.ConfigurePendingEmail(builder);

        builder.Property(x => x.UserType);
        builder.Property(x => x.FullName).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");
        builder
            .HasOne(x => x.OnboardingRecord)
            .WithOne(x => x.ResultingUser)
            .HasForeignKey<UserOnboardingRecord>(x => x.ResultingUserId);

        builder.Property(p => p.Version).IsRowVersion();

        builder.Ignore(x => x.Events);
    }
}
