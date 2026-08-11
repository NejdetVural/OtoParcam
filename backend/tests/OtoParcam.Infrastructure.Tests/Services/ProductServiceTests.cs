using OtoParcam.Application.Products;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_NonAdmin_OnlyReturnsAvailable_EvenIfStatusFilterRequested()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Hidden),
            CreateProduct(category, model, ProductStatus.Sold));
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var result = await service.GetProductsAsync(new ProductListQuery { Status = ProductStatus.Hidden }, isAdmin: false);

        var item = Assert.Single(result.Items);
        Assert.Equal(ProductStatus.Available, item.Status);
    }

    [Fact]
    public async Task GetProductsAsync_Admin_WithoutStatusFilter_ReturnsEveryStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Hidden),
            CreateProduct(category, model, ProductStatus.Sold));
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: true);

        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetProductsAsync_Admin_WithStatusFilter_ReturnsOnlyThatStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Hidden),
            CreateProduct(category, model, ProductStatus.Sold));
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        var result = await service.GetProductsAsync(new ProductListQuery { Status = ProductStatus.Sold }, isAdmin: true);

        var item = Assert.Single(result.Items);
        Assert.Equal(ProductStatus.Sold, item.Status);
    }

    [Fact]
    public async Task HideProductAsync_MovesAvailableProductToHidden()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.HideProductAsync(product.Id);

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Hidden, (await context.Products.FindAsync(product.Id))!.Status);
    }

    [Fact]
    public async Task HideProductAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.HideProductAsync(Guid.NewGuid());

        Assert.Equal(ProductOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RestoreProductAsync_FromHidden_MovesBackToAvailable()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Hidden);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.RestoreProductAsync(product.Id);

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Available, (await context.Products.FindAsync(product.Id))!.Status);
    }

    [Theory]
    [InlineData(ProductStatus.Available)]
    [InlineData(ProductStatus.Sold)]
    public async Task RestoreProductAsync_FromNonHiddenStatus_ReturnsInvalidStatusAndLeavesItUnchanged(ProductStatus status)
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, status);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.RestoreProductAsync(product.Id);

        Assert.Equal(ProductOperationStatus.InvalidStatus, result.Status);
        Assert.Equal(status, (await context.Products.FindAsync(product.Id))!.Status);
    }

    [Fact]
    public async Task RestoreProductAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.RestoreProductAsync(Guid.NewGuid());

        Assert.Equal(ProductOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetProductsAsync_SoldProduct_UsesNegotiatedPriceFromApprovedRequest()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Sold, price: 1000m);
        context.Products.Add(product);
        CreateApprovedSale(context, product, originalPrice: 1000m, negotiatedPrice: 1500m);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: true);

        var dto = Assert.Single(result.Items);
        Assert.Equal(1500m, dto.SoldPrice);
    }

    [Fact]
    public async Task GetProductsAsync_SoldProduct_FallsBackToOriginalPriceWhenNeverNegotiated()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Sold);
        context.Products.Add(product);
        CreateApprovedSale(context, product, originalPrice: 800m, negotiatedPrice: null);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: true);

        var dto = Assert.Single(result.Items);
        Assert.Equal(800m, dto.SoldPrice);
    }

    [Fact]
    public async Task GetProductsAsync_AvailableProduct_HasNullSoldPrice()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: true);

        var dto = Assert.Single(result.Items);
        Assert.Null(dto.SoldPrice);
    }

    [Fact]
    public async Task GetProductsAsync_NonAdmin_HidesAcquisitionCostAndSource()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available, acquisitionCost: 500m, acquisitionSource: "ABC Hurdacılık"));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: false);

        var dto = Assert.Single(result.Items);
        Assert.Null(dto.AcquisitionCost);
        Assert.Null(dto.AcquisitionSource);
    }

    [Fact]
    public async Task GetProductsAsync_Admin_KeepsAcquisitionCostAndSource()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available, acquisitionCost: 500m, acquisitionSource: "ABC Hurdacılık"));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.GetProductsAsync(new ProductListQuery(), isAdmin: true);

        var dto = Assert.Single(result.Items);
        Assert.Equal(500m, dto.AcquisitionCost);
        Assert.Equal("ABC Hurdacılık", dto.AcquisitionSource);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonAdmin_HidesAcquisitionCostAndSource()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, acquisitionCost: 500m, acquisitionSource: "ABC Hurdacılık");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = await service.GetProductByIdAsync(product.Id, isAdmin: false);

        Assert.NotNull(dto);
        Assert.Null(dto!.AcquisitionCost);
        Assert.Null(dto.AcquisitionSource);
    }

    [Fact]
    public async Task GetProductByIdAsync_Admin_KeepsAcquisitionCostAndSource()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, acquisitionCost: 500m, acquisitionSource: "ABC Hurdacılık");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = await service.GetProductByIdAsync(product.Id, isAdmin: true);

        Assert.NotNull(dto);
        Assert.Equal(500m, dto!.AcquisitionCost);
        Assert.Equal("ABC Hurdacılık", dto.AcquisitionSource);
    }

    [Fact]
    public async Task GetProductByIdAsync_Admin_WithBatchAndNoOverride_ComputesEffectiveCostFromEvenSplit()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);

        var target = CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch);
        context.Products.AddRange(
            target,
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch),
            CreateProduct(category, model, ProductStatus.Sold, acquisitionBatch: batch),
            CreateProduct(category, model, ProductStatus.Hidden, acquisitionBatch: batch));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = await service.GetProductByIdAsync(target.Id, isAdmin: true);

        Assert.NotNull(dto);
        Assert.Null(dto!.AcquisitionCost);
        Assert.Equal(250m, dto.EffectiveAcquisitionCost);
        Assert.Equal("Ford Focus - sigorta hasarlı", dto.EffectiveAcquisitionSource);
        Assert.Equal(batch.Id, dto.AcquisitionBatchId);
    }

    [Fact]
    public async Task GetProductByIdAsync_Admin_WithBatchAndOverride_OverrideWinsOverSplit()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);

        var target = CreateProduct(category, model, ProductStatus.Available, acquisitionCost: 700m, acquisitionSource: "Motor - ayrı değerli", acquisitionBatch: batch);
        context.Products.AddRange(
            target,
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = await service.GetProductByIdAsync(target.Id, isAdmin: true);

        Assert.NotNull(dto);
        Assert.Equal(700m, dto!.EffectiveAcquisitionCost);
        Assert.Equal("Motor - ayrı değerli", dto.EffectiveAcquisitionSource);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonAdmin_HidesBatchAndEffectiveFields()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);
        var product = CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = await service.GetProductByIdAsync(product.Id, isAdmin: false);

        Assert.NotNull(dto);
        Assert.Null(dto!.AcquisitionBatchId);
        Assert.Null(dto.AcquisitionBatchSource);
        Assert.Null(dto.EffectiveAcquisitionCost);
        Assert.Null(dto.EffectiveAcquisitionSource);
    }

    [Fact]
    public async Task CreateProductAsync_NoStatusSpecified_DefaultsToAvailable()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var service = new ProductService(context);
        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            CategoryId = category.Id,
            SourceVehicleModelId = model.Id,
            Color = ProductColor.Black,
        });

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Available, result.Product!.Status);
    }

    [Fact]
    public async Task CreateProductAsync_StatusHidden_CreatesAsHiddenInventoryOnlyItem()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var service = new ProductService(context);
        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            CategoryId = category.Id,
            SourceVehicleModelId = model.Id,
            Color = ProductColor.Black,
            Status = ProductStatus.Hidden,
        });

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Hidden, result.Product!.Status);
    }

    [Fact]
    public async Task CreateProductAsync_StatusSold_ReturnsInvalidStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var service = new ProductService(context);
        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            CategoryId = category.Id,
            SourceVehicleModelId = model.Id,
            Color = ProductColor.Black,
            Status = ProductStatus.Sold,
        });

        Assert.Equal(ProductOperationStatus.InvalidStatus, result.Status);
    }

    [Theory]
    [InlineData(ProductStatus.Available)]
    [InlineData(ProductStatus.Hidden)]
    public async Task MarkProductSoldAsync_FromAvailableOrHidden_SetsSoldStatusAndPrice(ProductStatus initialStatus)
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, initialStatus, price: 1000m);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.MarkProductSoldAsync(product.Id, new MarkProductSoldRequest { SoldPrice = 850m });

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Sold, result.Product!.Status);
        Assert.Equal(850m, result.Product.SoldPrice);

        var stored = await context.Products.FindAsync(product.Id);
        Assert.Equal(ProductStatus.Sold, stored!.Status);
        Assert.Equal(850m, stored.SoldPrice);
    }

    [Fact]
    public async Task MarkProductSoldAsync_AlreadySold_ReturnsInvalidStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Sold, price: 1000m);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.MarkProductSoldAsync(product.Id, new MarkProductSoldRequest { SoldPrice = 850m });

        Assert.Equal(ProductOperationStatus.InvalidStatus, result.Status);
    }

    [Fact]
    public async Task MarkProductSoldAsync_NegativePrice_ReturnsInvalidPrice()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.MarkProductSoldAsync(product.Id, new MarkProductSoldRequest { SoldPrice = -1m });

        Assert.Equal(ProductOperationStatus.InvalidPrice, result.Status);
    }

    [Fact]
    public async Task MarkProductSoldAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.MarkProductSoldAsync(Guid.NewGuid(), new MarkProductSoldRequest { SoldPrice = 100m });

        Assert.Equal(ProductOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateProductAsync_UnknownAcquisitionBatch_ReturnsInvalidAcquisitionBatch()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var service = new ProductService(context);
        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            CategoryId = category.Id,
            SourceVehicleModelId = model.Id,
            Color = ProductColor.Black,
            AcquisitionBatchId = Guid.NewGuid(),
        });

        Assert.Equal(ProductOperationStatus.InvalidAcquisitionBatch, result.Status);
    }

    [Fact]
    public async Task CreateProductAsync_ValidAcquisitionBatch_LinksProductAndSplitsCostAcrossExistingParts()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı", totalCost: 900m);
        context.AcquisitionBatches.Add(batch);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch));
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            CategoryId = category.Id,
            SourceVehicleModelId = model.Id,
            Color = ProductColor.Black,
            AcquisitionBatchId = batch.Id,
        });

        Assert.Equal(ProductOperationStatus.Success, result.Status);
        Assert.Equal(batch.Id, result.Product!.AcquisitionBatchId);
        Assert.Equal(450m, result.Product.EffectiveAcquisitionCost);
    }
}
