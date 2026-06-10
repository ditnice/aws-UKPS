using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.Identity;
using UKPS.Data.Enums;

namespace UKPS.Data.Configurations.Identity;

public class TermsAcceptanceConfiguration : IEntityTypeConfiguration<TermsAcceptance>
{
    public void Configure(EntityTypeBuilder<TermsAcceptance> builder)
    {
        builder.ToTable("terms_acceptance", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RelevantPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.SignatoryName).IsRequired();
        builder.Property(x => x.SignatoryEmail).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.LinkExpiresAt).HasColumnType("timestamptz");
        builder.Property(x => x.SignedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Organisation)
               .WithMany(x => x.TermsAcceptances)
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
