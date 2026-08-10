using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserOnboardingRecordConfiguration
    : IEntityTypeConfiguration<UserOnboardingRecord>
{
    public void Configure(EntityTypeBuilder<UserOnboardingRecord> builder)
    {
        builder.HasKey(x => x.SetupToken);
    }
}
