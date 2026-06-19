using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Configurations.Identity;

internal sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.OrganisationName).IsRequired();
        builder.Property(x => x.OrganisationType);
        // PharmaceuticalEntity is a [Flags] integer — stored as int, not a PG enum
        builder.Property(x => x.AllowedPharmaceuticalEntity);
        builder.Property(x => x.Status);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.LastActive).HasColumnType("timestamptz");

    }
}
