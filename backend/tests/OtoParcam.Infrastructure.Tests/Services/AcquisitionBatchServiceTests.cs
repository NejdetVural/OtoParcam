using OtoParcam.Application.AcquisitionBatches;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class AcquisitionBatchServiceTests
{
    [Fact]
    public async Task GetBatchByIdAsync_RollsUpPartCountsAndRevenueAcrossLinkedProducts()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı lotu", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);

        var sold = CreateProduct(category, model, ProductStatus.Sold, acquisitionBatch: batch);
        context.Products.AddRange(
            sold,
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch),
            CreateProduct(category, model, ProductStatus.Hidden, acquisitionBatch: batch));
        CreateApprovedSale(context, sold, originalPrice: 700m, negotiatedPrice: null);
        await context.SaveChangesAsync();

        var service = new AcquisitionBatchService(context);
        var dto = await service.GetBatchByIdAsync(batch.Id);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.PartCount);
        Assert.Equal(1, dto.SoldCount);
        Assert.Equal(1, dto.AvailableCount);
        Assert.Equal(1, dto.HiddenCount);
        Assert.Equal(1000m / 3, dto.EstimatedCostPerPart);
        Assert.Equal(700m, dto.RevenueSoFar);
        Assert.Equal(700m - 1000m, dto.ProfitSoFar);
    }

    [Fact]
    public async Task GetBatchByIdAsync_WithNoLinkedProducts_HasNullEstimatedCostPerPart()
    {
        await using var context = CreateContext();
        var batch = CreateBatch("Boş lot", totalCost: 500m);
        context.AcquisitionBatches.Add(batch);
        await context.SaveChangesAsync();

        var service = new AcquisitionBatchService(context);
        var dto = await service.GetBatchByIdAsync(batch.Id);

        Assert.NotNull(dto);
        Assert.Equal(0, dto!.PartCount);
        Assert.Null(dto.EstimatedCostPerPart);
    }

    [Fact]
    public async Task GetBatchByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new AcquisitionBatchService(context);

        var dto = await service.GetBatchByIdAsync(Guid.NewGuid());

        Assert.Null(dto);
    }

    [Fact]
    public async Task CreateBatchAsync_NegativeTotalCost_ReturnsInvalidCost()
    {
        await using var context = CreateContext();
        var service = new AcquisitionBatchService(context);

        var result = await service.CreateBatchAsync(new CreateAcquisitionBatchRequest
        {
            Source = "Test",
            TotalCost = -1m,
            PurchaseDate = DateTime.UtcNow,
        });

        Assert.Equal(AcquisitionBatchOperationStatus.InvalidCost, result.Status);
    }

    [Fact]
    public async Task DeleteBatchAsync_WithLinkedProducts_ReturnsConflict()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı lotu", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch));
        await context.SaveChangesAsync();

        var service = new AcquisitionBatchService(context);
        var result = await service.DeleteBatchAsync(batch.Id);

        Assert.Equal(AcquisitionBatchOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteBatchAsync_WithNoLinkedProducts_Succeeds()
    {
        await using var context = CreateContext();
        var batch = CreateBatch("Boş lot", totalCost: 500m);
        context.AcquisitionBatches.Add(batch);
        await context.SaveChangesAsync();

        var service = new AcquisitionBatchService(context);
        var result = await service.DeleteBatchAsync(batch.Id);

        Assert.Equal(AcquisitionBatchOperationStatus.Success, result.Status);
        Assert.Null(await context.AcquisitionBatches.FindAsync(batch.Id));
    }

    [Fact]
    public async Task DeleteBatchAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new AcquisitionBatchService(context);

        var result = await service.DeleteBatchAsync(Guid.NewGuid());

        Assert.Equal(AcquisitionBatchOperationStatus.NotFound, result.Status);
    }
}
