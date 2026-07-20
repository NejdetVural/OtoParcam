using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class VehicleModel : BaseEntity
{
    public Guid VehicleBrandId { get; set; }
    public string Name { get; set; } = null!;
    public short StartYear { get; set; }
    public short EndYear { get; set; }
    public string? Variant { get; set; }

    // Navigation properties
    public VehicleBrand VehicleBrand { get; set; } = null!;
    public ICollection<Product> SourceProducts { get; set; } = new List<Product>();
    public ICollection<ProductCompatibility> Compatibilities { get; set; } = new List<ProductCompatibility>();
}