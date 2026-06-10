using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisation", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.OrganisationName).IsRequired();
        builder.Property(x => x.OrganisationType).HasConversion<string>();
        // PharmaceuticalEntity is a [Flags] integer — stored as int, not a PG enum
        builder.Property(x => x.AllowedPharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");

    }
}
