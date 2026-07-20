using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class ProductCompatibility : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid VehicleModelId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public VehicleModel VehicleModel { get; set; } = null!;
}