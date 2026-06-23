using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

internal sealed class TermsAcceptanceConfiguration : IEntityTypeConfiguration<TermsAcceptance>
{
    public void Configure(EntityTypeBuilder<TermsAcceptance> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RelevantPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.SignatoryName).IsRequired();
        builder.Property(x => x.SignatoryEmail).IsRequired();
        builder.Property(x => x.Status);
        builder.Property(x => x.LinkExpiresAt).HasColumnType("timestamptz");
        builder.Property(x => x.SignedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Organisation)
               .WithMany(x => x.TermsAcceptances)
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
