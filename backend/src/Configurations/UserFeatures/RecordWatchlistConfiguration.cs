using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.UserFeatures;

namespace UKPS.Api.Configurations.UserFeatures;

internal sealed class RecordWatchlistConfiguration : IEntityTypeConfiguration<RecordWatchlist>
{
    public void Configure(EntityTypeBuilder<RecordWatchlist> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RecordId });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Record)
               .WithMany()
               .HasForeignKey(x => x.RecordId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
