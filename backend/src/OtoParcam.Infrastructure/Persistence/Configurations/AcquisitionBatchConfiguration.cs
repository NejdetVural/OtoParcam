using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class AcquisitionBatchConfiguration : IEntityTypeConfiguration<AcquisitionBatch>
{
    public void Configure(EntityTypeBuilder<AcquisitionBatch> builder)
    {
        builder.Property(b => b.Source)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.TotalCost)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_AcquisitionBatch_TotalCost", "[TotalCost] >= 0");
        });
    }
}
