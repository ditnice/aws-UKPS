using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesProductDetailTherapeuticAreaConfiguration
    : IEntityTypeConfiguration<MedicinesProductDetailTherapeuticArea>
{
    public void Configure(EntityTypeBuilder<MedicinesProductDetailTherapeuticArea> builder)
    {
        builder.HasKey(x => new { x.MedicinesProductDetailId, x.TherapeuticAreaId });

        builder
            .HasOne(x => x.MedicinesProductDetail)
            .WithMany(x => x.TherapeuticAreas)
            .HasForeignKey(x => x.MedicinesProductDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.TherapeuticArea)
            .WithMany()
            .HasForeignKey(x => x.TherapeuticAreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
