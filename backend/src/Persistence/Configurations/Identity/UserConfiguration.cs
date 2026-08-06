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
        builder.Property(x => x.IdentityId).HasMaxLength(36).HasColumnType("varchar(36)");
        builder.Property(x => x.WorkEmail).IsRequired();
        builder.HasIndex(x => x.WorkEmail).IsUnique().HasDatabaseName("ix_app_user_work_email");
        builder.Property(x => x.UserType);
        builder.Property(x => x.FullName).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");
        builder
            .HasOne(x => x.OnboardingRecord)
            .WithOne(x => x.User)
            .HasForeignKey<UserOnboardingRecord>(x => x.Id);
    }
}
