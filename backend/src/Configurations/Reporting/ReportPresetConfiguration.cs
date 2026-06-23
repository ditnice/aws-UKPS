using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.Reporting;

namespace UKPS.Api.Configurations.Reporting;

internal sealed class ReportPresetConfiguration : IEntityTypeConfiguration<ReportPreset>
{
    public void Configure(EntityTypeBuilder<ReportPreset> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ApplicableUserType);
        // PharmaceuticalEntity is a [Flags] integer
        builder.Property(x => x.ApplicablePharmaceuticalEntity).HasConversion<int>();
        builder.Property(x => x.Title).IsRequired();
        builder.ComplexProperty(x => x.Configuration, configuration =>
        {
            configuration.ToJson();
            configuration.IsRequired();
        });
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
