using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserPasswordResetRequestConfiguration
    : IEntityTypeConfiguration<UserPasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<UserPasswordResetRequest> builder)
    {
        builder.ToTable("user_password_reset_requests");
        builder.HasKey(x => x.TokenHash);
        builder.Property(x => x.TokenHash).ValueGeneratedNever();
        builder.HasIndex(x => new { x.TargetEmailAddress, x.CreatedAt });
    }
}
