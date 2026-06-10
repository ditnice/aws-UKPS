using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

public class RecordMhraProcedureConfiguration : IEntityTypeConfiguration<RecordMhraProcedure>
{
    public void Configure(EntityTypeBuilder<RecordMhraProcedure> builder)
    {
        builder.ToTable("record_mhra_procedure", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_mhra_procedure_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MhraProcedureType)
               .WithMany()
               .HasForeignKey(x => x.MhraProcedureTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
