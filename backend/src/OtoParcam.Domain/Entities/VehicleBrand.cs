using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class VehicleBrand : BaseEntity
{
    public string Name { get; set; } = null!;

    // Navigation property
    public ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
}