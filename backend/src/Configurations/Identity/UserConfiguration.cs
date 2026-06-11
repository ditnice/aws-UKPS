using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table is named app_user to avoid collision with PostgreSQL reserved word 'user'
        builder.ToTable("app_user");
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
