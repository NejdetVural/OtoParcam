using OtoParcam.Application.VehicleModels;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class VehicleModelServiceTests
{
    [Fact]
    public async Task CreateVehicleModelAsync_EndYearBeforeStartYear_ReturnsInvalidYearRange()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleModelService(context);
        var result = await service.CreateVehicleModelAsync(new CreateVehicleModelRequest
        {
            VehicleBrandId = model.VehicleBrandId,
            Name = "Egea",
            StartYear = 2020,
            EndYear = 2015,
        });

        Assert.Equal(VehicleModelOperationStatus.InvalidYearRange, result.Status);
    }

    [Fact]
    public async Task CreateVehicleModelAsync_UnknownVehicleBrand_ReturnsInvalidVehicleBrand()
    {
        await using var context = CreateContext();
        var service = new VehicleModelService(context);

        var result = await service.CreateVehicleModelAsync(new CreateVehicleModelRequest
        {
            VehicleBrandId = Guid.NewGuid(),
            Name = "Egea",
            StartYear = 2015,
            EndYear = 2023,
        });

        Assert.Equal(VehicleModelOperationStatus.InvalidVehicleBrand, result.Status);
    }

    [Fact]
    public async Task CreateVehicleModelAsync_ValidRequest_ReturnsSuccess()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleModelService(context);
        var result = await service.CreateVehicleModelAsync(new CreateVehicleModelRequest
        {
            VehicleBrandId = model.VehicleBrandId,
            Name = "Egea",
            StartYear = 2015,
            EndYear = 2023,
        });

        Assert.Equal(VehicleModelOperationStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.VehicleModel!.Id);
    }

    [Fact]
    public async Task UpdateVehicleModelAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleModelService(context);
        var result = await service.UpdateVehicleModelAsync(Guid.NewGuid(), new UpdateVehicleModelRequest
        {
            VehicleBrandId = model.VehicleBrandId,
            Name = "Egea",
            StartYear = 2015,
            EndYear = 2023,
        });

        Assert.Equal(VehicleModelOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleModelAsync_InvalidYearRangeAndUnknownBrand_ReturnsInvalidYearRangeFirst()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleModelService(context);
        var result = await service.UpdateVehicleModelAsync(model.Id, new UpdateVehicleModelRequest
        {
            VehicleBrandId = Guid.NewGuid(),
            Name = "Egea",
            StartYear = 2020,
            EndYear = 2015,
        });

        Assert.Equal(VehicleModelOperationStatus.InvalidYearRange, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleModelAsync_UnknownVehicleBrand_ReturnsInvalidVehicleBrand()
    {
        await using var context = CreateContext();
        var (_, model) = SeedCatalog(context);

        var service = new VehicleModelService(context);
        var result = await service.UpdateVehicleModelAsync(model.Id, new UpdateVehicleModelRequest
        {
            VehicleBrandId = Guid.NewGuid(),
            Name = "Egea",
            StartYear = 2015,
            EndYear = 2023,
        });

        Assert.Equal(VehicleModelOperationStatus.InvalidVehicleBrand, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleModelAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new VehicleModelService(context);

        var result = await service.DeleteVehicleModelAsync(Guid.NewGuid());

        Assert.Equal(VehicleModelOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleModelAsync_ReferencedAsProductSource_ReturnsConflict()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available));
        await context.SaveChangesAsync();

        var service = new VehicleModelService(context);
        var result = await service.DeleteVehicleModelAsync(model.Id);

        Assert.Equal(VehicleModelOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleModelAsync_ReferencedOnlyByCompatibility_ReturnsConflict()
    {
        await using var context = CreateContext();
        var (category, sourceModel) = SeedCatalog(context);
        var compatibleModel = new VehicleModel
        {
            Id = Guid.NewGuid(),
            VehicleBrandId = sourceModel.VehicleBrandId,
            Name = "Egea Cross",
            StartYear = 2018,
            EndYear = 2023,
        };
        context.VehicleModels.Add(compatibleModel);

        var product = CreateProduct(category, sourceModel, ProductStatus.Available);
        context.Products.Add(product);
        context.ProductCompatibilities.Add(new ProductCompatibility { ProductId = product.Id, VehicleModelId = compatibleModel.Id });
        await context.SaveChangesAsync();

        var service = new VehicleModelService(context);
        var result = await service.DeleteVehicleModelAsync(compatibleModel.Id);

        Assert.Equal(VehicleModelOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleModelAsync_NoReferences_RemovesModel()
    {
        await using var context = CreateContext();
        var (_, seededModel) = SeedCatalog(context);
        var unreferenced = new VehicleModel
        {
            Id = Guid.NewGuid(),
            VehicleBrandId = seededModel.VehicleBrandId,
            Name = "Tipo",
            StartYear = 2016,
            EndYear = 2020,
        };
        context.VehicleModels.Add(unreferenced);
        await context.SaveChangesAsync();

        var service = new VehicleModelService(context);
        var result = await service.DeleteVehicleModelAsync(unreferenced.Id);

        Assert.Equal(VehicleModelOperationStatus.Success, result.Status);
        Assert.Null(await context.VehicleModels.FindAsync(unreferenced.Id));
    }
}
