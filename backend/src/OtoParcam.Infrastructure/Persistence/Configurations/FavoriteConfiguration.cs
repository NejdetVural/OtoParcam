using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasIndex(f => new { f.ApplicationUserId, f.ProductId })
            .IsUnique();

        builder.HasOne(f => f.ApplicationUser)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Product)
            .WithMany(p => p.Favorites)
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
