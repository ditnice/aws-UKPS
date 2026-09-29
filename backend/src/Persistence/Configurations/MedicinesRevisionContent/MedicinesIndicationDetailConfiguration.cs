using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesIndicationDetailConfiguration
    : IEntityTypeConfiguration<MedicinesIndicationDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesIndicationDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Indication).IsRequired();
        builder.Property(x => x.IndicationIsPaediatric);
        builder.Property(x => x.IndicationIsCancer);
        builder.Property(x => x.IndicationIsRareDisease);
        builder.Property(x => x.IsPersonalisedMedicine);
        builder.Property(x => x.MedicineTechnologyStatus);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_indication_detail_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.BnfChapter)
            .WithMany()
            .HasForeignKey(x => x.BnfChapterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.FormulationType)
            .WithMany()
            .HasForeignKey(x => x.FormulationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
