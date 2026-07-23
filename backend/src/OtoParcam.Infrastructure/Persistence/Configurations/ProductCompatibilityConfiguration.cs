using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class ProductCompatibilityConfiguration : IEntityTypeConfiguration<ProductCompatibility>
{
    public void Configure(EntityTypeBuilder<ProductCompatibility> builder)
    {
        builder.HasIndex(pc => new { pc.ProductId, pc.VehicleModelId })
            .IsUnique();

        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.Compatibilities)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.VehicleModel)
            .WithMany(m => m.Compatibilities)
            .HasForeignKey(pc => pc.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
