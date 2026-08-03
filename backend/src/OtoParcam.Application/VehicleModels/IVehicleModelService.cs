namespace OtoParcam.Application.VehicleModels;

public interface IVehicleModelService
{
    Task<IReadOnlyList<VehicleModelDto>> GetVehicleModelsAsync(CancellationToken cancellationToken = default);
    Task<VehicleModelResult> CreateVehicleModelAsync(CreateVehicleModelRequest request, CancellationToken cancellationToken = default);
    Task<VehicleModelResult> UpdateVehicleModelAsync(Guid id, UpdateVehicleModelRequest request, CancellationToken cancellationToken = default);
    Task<VehicleModelDeleteResult> DeleteVehicleModelAsync(Guid id, CancellationToken cancellationToken = default);
}
