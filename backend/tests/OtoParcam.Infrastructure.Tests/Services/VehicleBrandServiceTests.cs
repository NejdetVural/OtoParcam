using OtoParcam.Application.VehicleBrands;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class VehicleBrandServiceTests
{
    [Fact]
    public async Task GetVehicleBrandsAsync_ReturnsBrands_OrderedAlphabetically()
    {
        await using var context = CreateContext();
        context.VehicleBrands.AddRange(
            new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Renault" },
            new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "BMW" },
            new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" });
        await context.SaveChangesAsync();

        var service = new VehicleBrandService(context);
        var result = await service.GetVehicleBrandsAsync();

        Assert.Equal(new[] { "BMW", "Fiat", "Renault" }, result.Select(b => b.Name));
    }

    [Fact]
    public async Task CreateVehicleBrandAsync_DuplicateName_ReturnsConflict()
    {
        await using var context = CreateContext();
        context.VehicleBrands.Add(new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" });
        await context.SaveChangesAsync();

        var service = new VehicleBrandService(context);
        var result = await service.CreateVehicleBrandAsync(new CreateVehicleBrandRequest { Name = "Fiat" });

        Assert.Equal(VehicleBrandOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CreateVehicleBrandAsync_NewName_ReturnsSuccessWithGeneratedId()
    {
        await using var context = CreateContext();
        var service = new VehicleBrandService(context);

        var result = await service.CreateVehicleBrandAsync(new CreateVehicleBrandRequest { Name = "Fiat" });

        Assert.Equal(VehicleBrandOperationStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.VehicleBrand!.Id);
        Assert.Equal("Fiat", result.VehicleBrand.Name);
    }

    [Fact]
    public async Task UpdateVehicleBrandAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new VehicleBrandService(context);

        var result = await service.UpdateVehicleBrandAsync(Guid.NewGuid(), new UpdateVehicleBrandRequest { Name = "Fiat" });

        Assert.Equal(VehicleBrandOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleBrandAsync_RenameToAnotherBrandsName_ReturnsConflict()
    {
        await using var context = CreateContext();
        var target = new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" };
        context.VehicleBrands.AddRange(target, new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "BMW" });
        await context.SaveChangesAsync();

        var service = new VehicleBrandService(context);
        var result = await service.UpdateVehicleBrandAsync(target.Id, new UpdateVehicleBrandRequest { Name = "BMW" });

        Assert.Equal(VehicleBrandOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleBrandAsync_RenameToOwnUnchangedName_DoesNotConflict()
    {
        await using var context = CreateContext();
        var target = new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" };
        context.VehicleBrands.Add(target);
        await context.SaveChangesAsync();

        var service = new VehicleBrandService(context);
        var result = await service.UpdateVehicleBrandAsync(target.Id, new UpdateVehicleBrandRequest { Name = "Fiat" });

        Assert.Equal(VehicleBrandOperationStatus.Success, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleBrandAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new VehicleBrandService(context);

        var result = await service.DeleteVehicleBrandAsync(Guid.NewGuid());

        Assert.Equal(VehicleBrandOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleBrandAsync_ReferencedByVehicleModel_ReturnsConflict()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleBrandService(context);
        var result = await service.DeleteVehicleBrandAsync(model.VehicleBrandId);

        Assert.Equal(VehicleBrandOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleBrandAsync_NoReferences_RemovesBrand()
    {
        await using var context = CreateContext();
        var brand = new Domain.Entities.VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" };
        context.VehicleBrands.Add(brand);
        await context.SaveChangesAsync();

        var service = new VehicleBrandService(context);
        var result = await service.DeleteVehicleBrandAsync(brand.Id);

        Assert.Equal(VehicleBrandOperationStatus.Success, result.Status);
        Assert.Null(await context.VehicleBrands.FindAsync(brand.Id));
    }
}
