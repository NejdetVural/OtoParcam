using OtoParcam.Domain.Common;
using OtoParcam.Domain.Enums;

namespace OtoParcam.Domain.Entities;

public class Product : BaseEntity
{ 
    public Guid CategoryId { get; set; }
    public Guid SourceVehicleModelId { get; set; }
    public decimal? Price { get; set; }
    public decimal? SoldPrice { get; set; }
    public DateTime? SoldAt { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? AcquisitionSource { get; set; }
    public Guid? AcquisitionBatchId { get; set; }
    public ProductColor Color { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public ProductSide? Side { get; set; }
    public ProductPosition? Position { get; set; }
    public string? Description { get; set; }

    // Navigation properties
     public Category Category { get; set; } = null!;
    public VehicleModel SourceVehicleModel { get; set; } = null!;
    public AcquisitionBatch? AcquisitionBatch { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<ProductCompatibility> Compatibilities { get; set; } = new List<ProductCompatibility>();
    public ICollection<PurchaseRequestItem> PurchaseRequestItem { get; set; } = new List<PurchaseRequestItem>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();


}