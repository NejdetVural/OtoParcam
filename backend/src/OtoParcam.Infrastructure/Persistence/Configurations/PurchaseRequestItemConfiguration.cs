using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class PurchaseRequestItemConfiguration : IEntityTypeConfiguration<PurchaseRequestItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestItem> builder)
    {
        builder.Property(i => i.OriginalPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.NegotiatedPrice)
            .HasColumnType("decimal(10,2)");

        builder.HasIndex(i => new { i.PurchaseRequestId, i.ProductId })
            .IsUnique();

        builder.HasOne(i => i.PurchaseRequest)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.PurchaseRequestItem)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
