using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Favorites;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class FavoriteServiceTests
{
    [Fact]
    public async Task AddFavoriteAsync_UnknownProduct_ReturnsProductNotFound()
    {
        await using var context = CreateContext();
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.AddFavoriteAsync(user.Id, new AddFavoriteRequest { ProductId = Guid.NewGuid() });

        Assert.Equal(FavoriteOperationStatus.ProductNotFound, result.Status);
    }

    [Fact]
    public async Task AddFavoriteAsync_Duplicate_ReturnsDuplicate()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        context.Favorites.Add(new Favorite { ApplicationUserId = user.Id, ProductId = product.Id });
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.AddFavoriteAsync(user.Id, new AddFavoriteRequest { ProductId = product.Id });

        Assert.Equal(FavoriteOperationStatus.Duplicate, result.Status);
    }

    [Fact]
    public async Task AddFavoriteAsync_Success_ReturnsProductWithBuiltTitleAndOrderedImages()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        context.ProductImages.AddRange(
            new ProductImage { ProductId = product.Id, ImageUrl = "second.jpg", DisplayOrder = 2 },
            new ProductImage { ProductId = product.Id, ImageUrl = "first.jpg", DisplayOrder = 1 });
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.AddFavoriteAsync(user.Id, new AddFavoriteRequest { ProductId = product.Id });

        Assert.Equal(FavoriteOperationStatus.Success, result.Status);
        Assert.Equal("Fiat Egea (2015-2023)", result.Product!.Title);
        Assert.Equal(new[] { "first.jpg", "second.jpg" }, result.Product.Images.Select(i => i.ImageUrl));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WrongUserOrProduct_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        var owner = CreateUser("owner@test.com");
        var otherUser = CreateUser("other@test.com");
        context.Users.AddRange(owner, otherUser);
        context.Favorites.Add(new Favorite { ApplicationUserId = owner.Id, ProductId = product.Id });
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.RemoveFavoriteAsync(otherUser.Id, product.Id);

        Assert.Equal(FavoriteOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_Success_RemovesFavoriteRow()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var product = CreateProduct(category, model, ProductStatus.Available);
        context.Products.Add(product);
        var user = CreateUser("customer@test.com");
        context.Users.Add(user);
        context.Favorites.Add(new Favorite { ApplicationUserId = user.Id, ProductId = product.Id });
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.RemoveFavoriteAsync(user.Id, product.Id);

        Assert.Equal(FavoriteOperationStatus.Success, result.Status);
        Assert.False(await context.Favorites.AnyAsync(f => f.ApplicationUserId == user.Id && f.ProductId == product.Id));
    }

    [Fact]
    public async Task GetFavoritesAsync_ScopedToCallingUser_OrderedByCreatedAtDescending()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var productA = CreateProduct(category, model, ProductStatus.Available);
        var productB = CreateProduct(category, model, ProductStatus.Available);
        context.Products.AddRange(productA, productB);
        var user = CreateUser("customer@test.com");
        var otherUser = CreateUser("other@test.com");
        context.Users.AddRange(user, otherUser);
        await context.SaveChangesAsync();

        context.Favorites.Add(new Favorite { ApplicationUserId = user.Id, ProductId = productA.Id });
        await context.SaveChangesAsync();
        await Task.Delay(20);
        context.Favorites.Add(new Favorite { ApplicationUserId = user.Id, ProductId = productB.Id });
        context.Favorites.Add(new Favorite { ApplicationUserId = otherUser.Id, ProductId = productA.Id });
        await context.SaveChangesAsync();

        var service = new FavoriteService(context);
        var result = await service.GetFavoritesAsync(user.Id);

        Assert.Equal(new[] { productB.Id, productA.Id }, result.Select(p => p.Id));
    }
}
