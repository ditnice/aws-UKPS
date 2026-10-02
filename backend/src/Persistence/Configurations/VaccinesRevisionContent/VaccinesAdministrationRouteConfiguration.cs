using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesAdministrationRouteConfiguration
    : IEntityTypeConfiguration<VaccinesAdministrationRoute>
{
    public void Configure(EntityTypeBuilder<VaccinesAdministrationRoute> builder)
    {
        builder.HasKey(x => new { x.VaccinesTechnologyId, x.AdministrationRouteId });

        builder
            .HasOne(x => x.VaccinesTechnology)
            .WithMany(x => x.AdministrationRoutes)
            .HasForeignKey(x => x.VaccinesTechnologyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AdministrationRoute)
            .WithMany()
            .HasForeignKey(x => x.AdministrationRouteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
