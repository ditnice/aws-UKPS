using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

/// <summary>
/// Shared base configuration for simple reference data tables
/// (id, label, is_archived).
/// </summary>
internal abstract class ReferenceDataBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : ReferenceDataBase
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Label).IsRequired();
    }
}
