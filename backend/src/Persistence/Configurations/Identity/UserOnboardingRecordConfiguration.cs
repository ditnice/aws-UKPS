using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserOnboardingRecordConfiguration
    : IEntityTypeConfiguration<UserOnboardingRecord>
{
    public void Configure(EntityTypeBuilder<UserOnboardingRecord> builder)
    {
        builder.ToTable("app_user");
        builder.Property(x => x.CreatedAt).HasColumnName("setup_token_created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("setup_token_record_created_by");
        builder.Property(x => x.ConsumedAt).HasColumnName("setup_token_record_consumed_at");
    }
}
