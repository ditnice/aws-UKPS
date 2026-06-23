using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesProductDetailConfiguration : IEntityTypeConfiguration<MedicinesProductDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesProductDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordTitle).IsRequired();
        builder.Property(x => x.Indication).IsRequired();
        builder.Property(x => x.IndicationIsPaediatric);
        builder.Property(x => x.IndicationIsCancer);

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_product_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BnfChapter)
               .WithMany()
               .HasForeignKey(x => x.BnfChapterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TherapeuticArea)
               .WithMany()
               .HasForeignKey(x => x.TherapeuticAreaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FormulationType)
               .WithMany()
               .HasForeignKey(x => x.FormulationTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
