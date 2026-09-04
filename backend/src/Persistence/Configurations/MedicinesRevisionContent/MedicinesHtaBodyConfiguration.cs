using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesHtaBodyConfiguration : IEntityTypeConfiguration<MedicinesHtaBody>
{
    public void Configure(EntityTypeBuilder<MedicinesHtaBody> builder)
    {
        builder.HasKey(x => new { x.RecordHtaId, x.Assessor });
        builder.Property(x => x.Assessor);

        builder
            .HasOne(x => x.RecordHta)
            .WithMany(x => x.HtaBodies)
            .HasForeignKey(x => x.RecordHtaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
