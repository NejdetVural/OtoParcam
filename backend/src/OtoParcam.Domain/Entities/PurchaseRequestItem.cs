using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class PurchaseRequestItem : BaseEntity
{
    public Guid PurchaseRequestId { get; set; }
    public Guid ProductId { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? NegotiatedPrice { get; set; }

    // Navigation properties
    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public Product Product { get; set; } = null!;
}