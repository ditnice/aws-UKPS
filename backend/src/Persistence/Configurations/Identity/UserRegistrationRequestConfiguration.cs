using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Configurations.Identity;

internal sealed class UserRegistrationRequestConfiguration
    : IEntityTypeConfiguration<UserRegistrationRequest>
{
    public void Configure(EntityTypeBuilder<UserRegistrationRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.RejectedAt).HasColumnType("timestamptz");

        builder
            .HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.RejectedByUser)
            .WithMany()
            .HasForeignKey(x => x.RejectedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
