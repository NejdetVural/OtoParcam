using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.VehicleBrands;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class VehicleBrandService : IVehicleBrandService
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleBrandService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VehicleBrandDto>> GetVehicleBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleBrands
            .OrderBy(b => b.Name)
            .Select(b => new VehicleBrandDto { Id = b.Id, Name = b.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleBrandResult> CreateVehicleBrandAsync(CreateVehicleBrandRequest request, CancellationToken cancellationToken = default)
    {
        var nameExists = await _dbContext.VehicleBrands.AnyAsync(b => b.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            return VehicleBrandResult.Conflict("A vehicle brand with this name already exists.");
        }

        var vehicleBrand = new VehicleBrand { Name = request.Name };
        _dbContext.VehicleBrands.Add(vehicleBrand);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleBrandResult.Success(new VehicleBrandDto { Id = vehicleBrand.Id, Name = vehicleBrand.Name });
    }

    public async Task<VehicleBrandResult> UpdateVehicleBrandAsync(Guid id, UpdateVehicleBrandRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleBrand = await _dbContext.VehicleBrands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (vehicleBrand is null)
        {
            return VehicleBrandResult.NotFound();
        }

        var nameTaken = await _dbContext.VehicleBrands.AnyAsync(b => b.Id != id && b.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return VehicleBrandResult.Conflict("A vehicle brand with this name already exists.");
        }

        vehicleBrand.Name = request.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleBrandResult.Success(new VehicleBrandDto { Id = vehicleBrand.Id, Name = vehicleBrand.Name });
    }

    public async Task<VehicleBrandDeleteResult> DeleteVehicleBrandAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicleBrand = await _dbContext.VehicleBrands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (vehicleBrand is null)
        {
            return VehicleBrandDeleteResult.NotFound();
        }

        var hasVehicleModels = await _dbContext.VehicleModels.AnyAsync(m => m.VehicleBrandId == id, cancellationToken);
        if (hasVehicleModels)
        {
            return VehicleBrandDeleteResult.Conflict("Vehicle brand cannot be deleted while referenced by one or more vehicle models.");
        }

        _dbContext.VehicleBrands.Remove(vehicleBrand);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VehicleBrandDeleteResult.Success();
    }
}
