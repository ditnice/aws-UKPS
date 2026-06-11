using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesAdjuvantConfiguration : IEntityTypeConfiguration<VaccinesAdjuvant>
{
    public void Configure(EntityTypeBuilder<VaccinesAdjuvant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.AdjuvantName).IsRequired();

        builder.HasIndex(x => x.VaccinesTechnologyId)
               .HasDatabaseName("ix_vaccines_adjuvant_technology_id");

        builder.HasOne(x => x.VaccinesTechnology)
               .WithMany(x => x.Adjuvants)
               .HasForeignKey(x => x.VaccinesTechnologyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
