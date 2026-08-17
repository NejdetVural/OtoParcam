using Microsoft.AspNetCore.Identity;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class DashboardServiceTests
{
    private static IdentityRole<Guid> AddCustomerRole(ApplicationDbContext context)
    {
        var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = Roles.Customer, NormalizedName = Roles.Customer.ToUpperInvariant() };
        context.Roles.Add(role);
        return role;
    }

    private static void AssignRole(ApplicationDbContext context, ApplicationUser user, IdentityRole<Guid> role)
    {
        context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });
    }

    [Fact]
    public async Task GetDashboardStatsAsync_TotalProducts_CountsAllStatuses()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Hidden),
            CreateProduct(category, model, ProductStatus.Sold));
        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetDashboardStatsAsync();

        Assert.Equal(3, result.TotalProducts);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_TotalCustomers_CountsOnlyCustomerRoleAssignments()
    {
        await using var context = CreateContext();
        var customerRole = AddCustomerRole(context);
        var adminRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = Roles.Administrator, NormalizedName = Roles.Administrator.ToUpperInvariant() };
        context.Roles.Add(adminRole);

        var customer = CreateUser("customer@test.com");
        var admin = CreateUser("admin@test.com");
        context.Users.AddRange(customer, admin);
        AssignRole(context, customer, customerRole);
        AssignRole(context, admin, adminRole);
        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetDashboardStatsAsync();

        Assert.Equal(1, result.TotalCustomers);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_PendingPurchaseRequests_CountsOnlyPending()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        CreatePurchaseRequest(context, user.Id, PurchaseRequestStatus.Pending, product);
        CreatePurchaseRequest(context, user.Id, PurchaseRequestStatus.Approved, product);
        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetDashboardStatsAsync();

        Assert.Equal(1, result.PendingPurchaseRequests);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ProductsAwaitingAttention_CountsAvailableWithMissingImagesOrCompatibility()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var missingBoth = CreateProduct(category, model, ProductStatus.Available);
        var complete = CreateProduct(category, model, ProductStatus.Available);
        var hiddenMissingBoth = CreateProduct(category, model, ProductStatus.Hidden);
        context.Products.AddRange(missingBoth, complete, hiddenMissingBoth);
        context.ProductImages.Add(new ProductImage { ProductId = complete.Id, ImageUrl = "a.jpg", DisplayOrder = 1 });
        context.ProductCompatibilities.Add(new ProductCompatibility { ProductId = complete.Id, VehicleModelId = model.Id });
        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetDashboardStatsAsync();

        Assert.Equal(1, result.ProductsAwaitingAttention);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_AcquisitionBatchesInProgress_CountsOnlyBatchesWithAnAvailableProduct()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var inProgressBatch = CreateBatch("Batch A", totalCost: 1000m);
        var finishedBatch = CreateBatch("Batch B", totalCost: 500m);
        context.AcquisitionBatches.AddRange(inProgressBatch, finishedBatch);
        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: inProgressBatch),
            CreateProduct(category, model, ProductStatus.Sold, acquisitionBatch: finishedBatch));
        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetDashboardStatsAsync();

        Assert.Equal(1, result.AcquisitionBatchesInProgress);
    }
}
