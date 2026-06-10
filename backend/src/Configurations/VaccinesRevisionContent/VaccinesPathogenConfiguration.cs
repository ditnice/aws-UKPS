using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.VaccinesRevisionContent;

namespace UKPS.Data.Configurations.VaccinesRevisionContent;

public class VaccinesPathogenConfiguration : IEntityTypeConfiguration<VaccinesPathogen>
{
    public void Configure(EntityTypeBuilder<VaccinesPathogen> builder)
    {
        builder.ToTable("vaccines_pathogen", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.PathogenName).IsRequired();

        builder.HasIndex(x => x.VaccinesDiseaseDetailId)
               .HasDatabaseName("ix_vaccines_pathogen_disease_detail_id");

        builder.HasOne(x => x.VaccinesDiseaseDetail)
               .WithMany(x => x.Pathogens)
               .HasForeignKey(x => x.VaccinesDiseaseDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
