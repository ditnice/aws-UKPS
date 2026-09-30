using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesIndicationDetailTherapeuticAreaConfiguration
    : IEntityTypeConfiguration<MedicinesIndicationDetailTherapeuticArea>
{
    public void Configure(EntityTypeBuilder<MedicinesIndicationDetailTherapeuticArea> builder)
    {
        builder.HasKey(x => new { x.MedicinesIndicationDetailId, x.TherapeuticAreaId });

        builder
            .HasOne(x => x.MedicinesIndicationDetail)
            .WithMany(x => x.TherapeuticAreas)
            .HasForeignKey(x => x.MedicinesIndicationDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.TherapeuticArea)
            .WithMany()
            .HasForeignKey(x => x.TherapeuticAreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
