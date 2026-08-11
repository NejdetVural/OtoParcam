using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class AcquisitionBatch : BaseEntity
{
    public string Source { get; set; } = null!;
    public decimal TotalCost { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
