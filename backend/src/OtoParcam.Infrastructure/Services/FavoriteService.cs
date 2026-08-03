using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Favorites;
using OtoParcam.Application.Products;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _dbContext;

    public FavoriteService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductDto>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Favorites
            .Where(f => f.ApplicationUserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Include(f => f.Product).ThenInclude(p => p.Category)
            .Include(f => f.Product).ThenInclude(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(f => f.Product).ThenInclude(p => p.ProductImages)
            .Select(f => f.Product)
            .ToListAsync(cancellationToken);

        return products.Select(ToDto).ToList();
    }

    public async Task<FavoriteResult> AddFavoriteAsync(Guid userId, AddFavoriteRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return FavoriteResult.ProductNotFound();
        }

        var alreadyExists = await _dbContext.Favorites
            .AnyAsync(f => f.ApplicationUserId == userId && f.ProductId == request.ProductId, cancellationToken);
        if (alreadyExists)
        {
            return FavoriteResult.Duplicate("This product is already in the customer's favorites.");
        }

        _dbContext.Favorites.Add(new Favorite
        {
            ApplicationUserId = userId,
            ProductId = request.ProductId
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return FavoriteResult.Success(ToDto(product));
    }

    public async Task<FavoriteDeleteResult> RemoveFavoriteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.ApplicationUserId == userId && f.ProductId == productId, cancellationToken);
        if (favorite is null)
        {
            return FavoriteDeleteResult.NotFound();
        }

        _dbContext.Favorites.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return FavoriteDeleteResult.Success();
    }

    private static ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Title = BuildTitle(product.SourceVehicleModel),
        CategoryId = product.CategoryId,
        CategoryName = product.Category.Name,
        SourceVehicleModelId = product.SourceVehicleModelId,
        VehicleBrandId = product.SourceVehicleModel.VehicleBrandId,
        VehicleBrandName = product.SourceVehicleModel.VehicleBrand.Name,
        VehicleModelName = product.SourceVehicleModel.Name,
        StartYear = product.SourceVehicleModel.StartYear,
        EndYear = product.SourceVehicleModel.EndYear,
        Variant = product.SourceVehicleModel.Variant,
        Price = product.Price,
        Color = product.Color,
        Status = product.Status,
        Description = product.Description,
        Images = product.ProductImages
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl, DisplayOrder = i.DisplayOrder })
            .ToList()
    };

    private static string BuildTitle(VehicleModel vehicleModel)
    {
        var variantPart = string.IsNullOrWhiteSpace(vehicleModel.Variant) ? string.Empty : $" {vehicleModel.Variant}";
        return $"{vehicleModel.VehicleBrand.Name} {vehicleModel.Name}{variantPart} ({vehicleModel.StartYear}-{vehicleModel.EndYear})";
    }
}
