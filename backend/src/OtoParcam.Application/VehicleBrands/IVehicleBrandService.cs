namespace OtoParcam.Application.VehicleBrands;

public interface IVehicleBrandService
{
    Task<IReadOnlyList<VehicleBrandDto>> GetVehicleBrandsAsync(CancellationToken cancellationToken = default);
    Task<VehicleBrandResult> CreateVehicleBrandAsync(CreateVehicleBrandRequest request, CancellationToken cancellationToken = default);
    Task<VehicleBrandResult> UpdateVehicleBrandAsync(Guid id, UpdateVehicleBrandRequest request, CancellationToken cancellationToken = default);
    Task<VehicleBrandDeleteResult> DeleteVehicleBrandAsync(Guid id, CancellationToken cancellationToken = default);
}
