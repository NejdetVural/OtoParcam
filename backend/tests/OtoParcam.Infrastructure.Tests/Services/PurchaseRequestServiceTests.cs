using OtoParcam.Application.PurchaseRequests;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class PurchaseRequestServiceTests
{
    [Theory]
    [InlineData(PurchaseRequestStatus.Pending)]
    [InlineData(PurchaseRequestStatus.WaitingForCustomerConfirmation)]
    public async Task CreatePurchaseRequestAsync_SameCustomerAlreadyHasActiveRequestForProduct_ReturnsProductAlreadyRequested(PurchaseRequestStatus existingStatus)
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        CreatePurchaseRequest(context, user.Id, existingStatus, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        var result = await service.CreatePurchaseRequestAsync(user.Id, new CreatePurchaseRequestRequest { ProductIds = [product.Id] });

        Assert.Equal(PurchaseRequestOperationStatus.ProductAlreadyRequested, result.Status);
    }

    [Theory]
    [InlineData(PurchaseRequestStatus.Cancelled)]
    [InlineData(PurchaseRequestStatus.Rejected)]
    public async Task CreatePurchaseRequestAsync_PreviousRequestIsTerminal_AllowsNewRequest(PurchaseRequestStatus terminalStatus)
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        CreatePurchaseRequest(context, user.Id, terminalStatus, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        var result = await service.CreatePurchaseRequestAsync(user.Id, new CreatePurchaseRequestRequest { ProductIds = [product.Id] });

        Assert.Equal(PurchaseRequestOperationStatus.Success, result.Status);
    }

    [Fact]
    public async Task CreatePurchaseRequestAsync_DifferentCustomerHasActiveRequest_StillAllowed()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var firstCustomer = CreateUser("first@test.com");
        var secondCustomer = CreateUser("second@test.com");
        context.Users.AddRange(firstCustomer, secondCustomer);
        CreatePurchaseRequest(context, firstCustomer.Id, PurchaseRequestStatus.Pending, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        var result = await service.CreatePurchaseRequestAsync(secondCustomer.Id, new CreatePurchaseRequestRequest { ProductIds = [product.Id] });

        Assert.Equal(PurchaseRequestOperationStatus.Success, result.Status);
    }

    [Fact]
    public async Task ConfirmPurchaseRequestAsync_ProductAlreadyClaimedByAnotherApprovedRequest_ReturnsProductNotAvailable()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var firstCustomer = CreateUser("first@test.com");
        var secondCustomer = CreateUser("second@test.com");
        context.Users.AddRange(firstCustomer, secondCustomer);
        var firstRequest = CreatePurchaseRequest(context, firstCustomer.Id, PurchaseRequestStatus.Pending, product);
        var secondRequest = CreatePurchaseRequest(context, secondCustomer.Id, PurchaseRequestStatus.Pending, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);

        var firstConfirm = await service.ConfirmPurchaseRequestAsync(firstCustomer.Id, firstRequest.Id);
        Assert.Equal(PurchaseRequestOperationStatus.Success, firstConfirm.Status);
        Assert.Equal(ProductStatus.Sold, (await context.Products.FindAsync(product.Id))!.Status);

        var secondConfirm = await service.ConfirmPurchaseRequestAsync(secondCustomer.Id, secondRequest.Id);

        Assert.Equal(PurchaseRequestOperationStatus.ProductNotAvailable, secondConfirm.Status);
        Assert.Equal(PurchaseRequestStatus.Pending, (await context.PurchaseRequests.FindAsync(secondRequest.Id))!.Status);
    }

    [Fact]
    public async Task ConfirmPurchaseRequestAsync_ProductStillAvailable_Succeeds()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        var request = CreatePurchaseRequest(context, user.Id, PurchaseRequestStatus.Pending, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        var result = await service.ConfirmPurchaseRequestAsync(user.Id, request.Id);

        Assert.Equal(PurchaseRequestOperationStatus.Success, result.Status);
        Assert.Equal(ProductStatus.Sold, (await context.Products.FindAsync(product.Id))!.Status);
    }

    [Fact]
    public async Task ConfirmPurchaseRequestAsync_NoNegotiation_SetsSoldPriceToOriginalPrice()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        var request = CreatePurchaseRequest(context, user.Id, PurchaseRequestStatus.Pending, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        var result = await service.ConfirmPurchaseRequestAsync(user.Id, request.Id);

        Assert.Equal(PurchaseRequestOperationStatus.Success, result.Status);
        Assert.Equal(500m, (await context.Products.FindAsync(product.Id))!.SoldPrice);
    }

    [Fact]
    public async Task ConfirmPurchaseRequestAsync_WithNegotiation_SetsSoldPriceToNegotiatedPrice()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available, price: 500m);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        var request = CreatePurchaseRequest(context, user.Id, PurchaseRequestStatus.Pending, product);
        await context.SaveChangesAsync();

        var service = new PurchaseRequestService(context);
        await service.UpdateNegotiationAsync(request.Id, new UpdateNegotiationRequest
        {
            Items = [new NegotiationItemRequest { ProductId = product.Id, NegotiatedPrice = 420m }],
        });

        var result = await service.ConfirmPurchaseRequestAsync(user.Id, request.Id);

        Assert.Equal(PurchaseRequestOperationStatus.Success, result.Status);
        Assert.Equal(420m, (await context.Products.FindAsync(product.Id))!.SoldPrice);
    }
}
