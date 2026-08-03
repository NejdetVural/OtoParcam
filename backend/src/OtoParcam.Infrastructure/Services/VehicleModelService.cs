using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.VehicleModels;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class VehicleModelService : IVehicleModelService
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleModelService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VehicleModelDto>> GetVehicleModelsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleModels
            .OrderBy(m => m.Name)
            .Select(m => ToDto(m))
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleModelResult> CreateVehicleModelAsync(CreateVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EndYear < request.StartYear)
        {
            return VehicleModelResult.InvalidYearRange("EndYear must be greater than or equal to StartYear.");
        }

        var brandExists = await _dbContext.VehicleBrands.AnyAsync(b => b.Id == request.VehicleBrandId, cancellationToken);
        if (!brandExists)
        {
            return VehicleModelResult.InvalidVehicleBrand("The specified vehicle brand does not exist.");
        }

        var vehicleModel = new VehicleModel
        {
            VehicleBrandId = request.VehicleBrandId,
            Name = request.Name,
            StartYear = request.StartYear,
            EndYear = request.EndYear,
            Variant = request.Variant
        };

        _dbContext.VehicleModels.Add(vehicleModel);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleModelResult.Success(ToDto(vehicleModel));
    }

    public async Task<VehicleModelResult> UpdateVehicleModelAsync(Guid id, UpdateVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleModel = await _dbContext.VehicleModels.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (vehicleModel is null)
        {
            return VehicleModelResult.NotFound();
        }

        if (request.EndYear < request.StartYear)
        {
            return VehicleModelResult.InvalidYearRange("EndYear must be greater than or equal to StartYear.");
        }

        var brandExists = await _dbContext.VehicleBrands.AnyAsync(b => b.Id == request.VehicleBrandId, cancellationToken);
        if (!brandExists)
        {
            return VehicleModelResult.InvalidVehicleBrand("The specified vehicle brand does not exist.");
        }

        vehicleModel.VehicleBrandId = request.VehicleBrandId;
        vehicleModel.Name = request.Name;
        vehicleModel.StartYear = request.StartYear;
        vehicleModel.EndYear = request.EndYear;
        vehicleModel.Variant = request.Variant;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleModelResult.Success(ToDto(vehicleModel));
    }

    public async Task<VehicleModelDeleteResult> DeleteVehicleModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicleModel = await _dbContext.VehicleModels.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (vehicleModel is null)
        {
            return VehicleModelDeleteResult.NotFound();
        }

        var isReferencedByProducts = await _dbContext.Products.AnyAsync(p => p.SourceVehicleModelId == id, cancellationToken);
        var isReferencedByCompatibility = await _dbContext.ProductCompatibilities.AnyAsync(c => c.VehicleModelId == id, cancellationToken);
        if (isReferencedByProducts || isReferencedByCompatibility)
        {
            return VehicleModelDeleteResult.Conflict("Vehicle model cannot be deleted while referenced by one or more products or compatibility records.");
        }

        _dbContext.VehicleModels.Remove(vehicleModel);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleModelDeleteResult.Success();
    }

    private static VehicleModelDto ToDto(VehicleModel vehicleModel) => new()
    {
        Id = vehicleModel.Id,
        VehicleBrandId = vehicleModel.VehicleBrandId,
        Name = vehicleModel.Name,
        StartYear = vehicleModel.StartYear,
        EndYear = vehicleModel.EndYear,
        Variant = vehicleModel.Variant
    };
}
