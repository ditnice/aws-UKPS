using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.VaccinesRevisionContent;

namespace UKPS.Data.Configurations.VaccinesRevisionContent;

public class VaccinesDiseaseDetailConfiguration : IEntityTypeConfiguration<VaccinesDiseaseDetail>
{
    public void Configure(EntityTypeBuilder<VaccinesDiseaseDetail> builder)
    {
        builder.ToTable("vaccines_disease_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DiseaseTarget).IsRequired();
        builder.Property(x => x.AgeGroup).IsRequired();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_disease_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DiseaseArea)
               .WithMany()
               .HasForeignKey(x => x.DiseaseAreaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
