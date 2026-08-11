using Microsoft.EntityFrameworkCore;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Tests;

internal static class TestFixtures
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    public static (Category Category, VehicleModel Model) SeedCatalog(ApplicationDbContext context)
    {
        var brand = new VehicleBrand { Id = Guid.NewGuid(), Name = "Fiat" };
        var model = new VehicleModel
        {
            Id = Guid.NewGuid(),
            VehicleBrandId = brand.Id,
            VehicleBrand = brand,
            Name = "Egea",
            StartYear = 2015,
            EndYear = 2023,
        };
        var category = new Category { Id = Guid.NewGuid(), Name = "Motor" };

        context.VehicleBrands.Add(brand);
        context.VehicleModels.Add(model);
        context.Categories.Add(category);
        context.SaveChanges();

        return (category, model);
    }

    public static Product CreateProduct(
        Category category,
        VehicleModel model,
        ProductStatus status,
        decimal? price = null,
        decimal? acquisitionCost = null,
        string? acquisitionSource = null,
        AcquisitionBatch? acquisitionBatch = null)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category,
            SourceVehicleModelId = model.Id,
            SourceVehicleModel = model,
            Color = ProductColor.Black,
            Status = status,
            Price = price,
            AcquisitionCost = acquisitionCost,
            AcquisitionSource = acquisitionSource,
            AcquisitionBatchId = acquisitionBatch?.Id,
            AcquisitionBatch = acquisitionBatch,
        };
    }

    public static AcquisitionBatch CreateBatch(string source, decimal totalCost, DateTime? purchaseDate = null)
    {
        return new AcquisitionBatch
        {
            Id = Guid.NewGuid(),
            Source = source,
            TotalCost = totalCost,
            PurchaseDate = purchaseDate ?? DateTime.UtcNow,
        };
    }

    public static ApplicationUser CreateUser(string email)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
        };
    }

    public static PurchaseRequest CreatePurchaseRequest(ApplicationDbContext context, Guid userId, PurchaseRequestStatus status, params Product[] products)
    {
        var purchaseRequest = new PurchaseRequest { Id = Guid.NewGuid(), ApplicationUserId = userId, Status = status };
        context.PurchaseRequests.Add(purchaseRequest);

        foreach (var product in products)
        {
            context.PurchaseRequestItems.Add(new PurchaseRequestItem
            {
                Id = Guid.NewGuid(),
                PurchaseRequestId = purchaseRequest.Id,
                ProductId = product.Id,
                OriginalPrice = product.Price,
            });
        }

        return purchaseRequest;
    }

    public static PurchaseRequestItem CreateApprovedSale(ApplicationDbContext context, Product product, decimal? originalPrice, decimal? negotiatedPrice)
    {
        var user = CreateUser($"{Guid.NewGuid()}@test.com");
        context.Users.Add(user);

        var purchaseRequest = new PurchaseRequest { Id = Guid.NewGuid(), ApplicationUserId = user.Id, Status = PurchaseRequestStatus.Approved };
        context.PurchaseRequests.Add(purchaseRequest);

        var item = new PurchaseRequestItem
        {
            Id = Guid.NewGuid(),
            PurchaseRequestId = purchaseRequest.Id,
            ProductId = product.Id,
            OriginalPrice = originalPrice,
            NegotiatedPrice = negotiatedPrice,
        };
        context.PurchaseRequestItems.Add(item);
        product.SoldPrice = negotiatedPrice ?? originalPrice;
        product.SoldAt = DateTime.UtcNow;

        return item;
    }
}
