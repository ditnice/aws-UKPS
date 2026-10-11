using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Data.ReferenceData;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class BnfChapterConfiguration : IEntityTypeConfiguration<BnfChapter>
{
    public void Configure(EntityTypeBuilder<BnfChapter> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Label).IsRequired();

        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            BnfChapterSeedData.Chapters.Select(chapter => new BnfChapter
            {
                Id = chapter.Id,
                Code = chapter.Code,
                Label = chapter.Label,
                DisplayOrder = chapter.Id,
            })
        );
    }
}
