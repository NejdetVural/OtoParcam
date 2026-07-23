using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Persistence.Configurations;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.HasOne(r => r.ApplicationUser)
            .WithMany(u => u.PurchaseRequests)
            .HasForeignKey(r => r.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
