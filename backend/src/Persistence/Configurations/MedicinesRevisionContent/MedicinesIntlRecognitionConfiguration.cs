using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesIntlRecognitionConfiguration
    : IEntityTypeConfiguration<MedicinesIntlRecognition>
{
    public void Configure(EntityTypeBuilder<MedicinesIntlRecognition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IntlConditionalApprovalAnticipated);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_intl_recognition_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.IrpReferenceRegulator)
            .WithMany()
            .HasForeignKey(x => x.IrpReferenceRegulatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.IrpRoute)
            .WithMany()
            .HasForeignKey(x => x.IrpRouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.IntlSubmissionDate)
            .WithMany()
            .HasForeignKey(x => x.IntlSubmissionDateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.IntlLicenceDate)
            .WithMany()
            .HasForeignKey(x => x.IntlLicenceDateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
