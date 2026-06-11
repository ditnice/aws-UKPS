using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

internal sealed class RecordHtaBodyConfiguration : IEntityTypeConfiguration<RecordHtaBody>
{
    public void Configure(EntityTypeBuilder<RecordHtaBody> builder)
    {
        builder.HasKey(x => new { x.RecordHtaId, x.Label });
        builder.Property(x => x.Label).IsRequired();

        builder.HasOne(x => x.RecordHta)
               .WithMany(x => x.HtaBodies)
               .HasForeignKey(x => x.RecordHtaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
