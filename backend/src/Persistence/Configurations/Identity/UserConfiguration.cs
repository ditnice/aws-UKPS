using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.figurations.Identity;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table is named app_user to avoid collision with PostgreSQL reserved word 'user'
        builder.ToTable("app_user");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Username).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique().HasDatabaseName("ix_app_user_username");
        builder.Property(x => x.WorkEmail).IsRequired();
        builder.HasIndex(x => x.WorkEmail).IsUnique().HasDatabaseName("ix_app_user_work_email");
        builder.Property(x => x.UserType);
        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();
    }
}
