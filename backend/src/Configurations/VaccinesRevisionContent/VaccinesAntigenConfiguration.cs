using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesAntigenConfiguration : IEntityTypeConfiguration<VaccinesAntigen>
{
    public void Configure(EntityTypeBuilder<VaccinesAntigen> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.AntigenName).IsRequired();

        builder.HasIndex(x => x.VaccinesTechnologyId)
               .HasDatabaseName("ix_vaccines_antigen_technology_id");

        builder.HasOne(x => x.VaccinesTechnology)
               .WithMany(x => x.Antigens)
               .HasForeignKey(x => x.VaccinesTechnologyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
