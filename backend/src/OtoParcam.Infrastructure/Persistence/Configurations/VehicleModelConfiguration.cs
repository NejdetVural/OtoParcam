using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Variant)
            .HasMaxLength(100);

        builder.HasIndex(m => m.Name);

        builder.HasOne(m => m.VehicleBrand)
            .WithMany(b => b.VehicleModels)
            .HasForeignKey(m => m.VehicleBrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
