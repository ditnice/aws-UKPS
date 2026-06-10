using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.VaccinesRevisionContent;

namespace UKPS.Data.Configurations.VaccinesRevisionContent;

public class VaccinesCompanyCodeConfiguration : IEntityTypeConfiguration<VaccinesCompanyCode>
{
    public void Configure(EntityTypeBuilder<VaccinesCompanyCode> builder)
    {
        builder.ToTable("vaccines_company_code", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).IsRequired();

        builder.HasIndex(x => x.VaccinesProductDetailId)
               .HasDatabaseName("ix_vaccines_company_code_product_detail_id");

        builder.HasOne(x => x.VaccinesProductDetail)
               .WithMany(x => x.CompanyCodes)
               .HasForeignKey(x => x.VaccinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
