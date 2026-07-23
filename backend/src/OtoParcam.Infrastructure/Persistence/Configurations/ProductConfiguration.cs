using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Color);

        builder.ToTable(t => t.HasCheckConstraint("CK_Product_Price", "[Price] >= 0"));

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SourceVehicleModel)
            .WithMany(m => m.SourceProducts)
            .HasForeignKey(p => p.SourceVehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
