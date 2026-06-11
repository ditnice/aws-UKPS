using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class BnfChapterConfiguration : IEntityTypeConfiguration<BnfChapter>
{
    public void Configure(EntityTypeBuilder<BnfChapter> builder)
    {
        builder.ToTable("bnf_chapter");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Label).IsRequired();

        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
