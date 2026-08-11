using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Products;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class ProductService : IProductService
{
    private const int PageSize = 20;

    private readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductListQuery query, bool isAdmin, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> products = _dbContext.Products;

        if (!isAdmin)
        {
            products = products.Where(p => p.Status == ProductStatus.Available);
        }
        else if (query.Status.HasValue)
        {
            products = products.Where(p => p.Status == query.Status.Value);
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.VehicleBrandId.HasValue)
        {
            products = products.Where(p =>
                p.SourceVehicleModel.VehicleBrandId == query.VehicleBrandId.Value ||
                p.Compatibilities.Any(c => c.VehicleModel.VehicleBrandId == query.VehicleBrandId.Value));
        }

        if (query.VehicleModelId.HasValue)
        {
            products = products.Where(p =>
                p.SourceVehicleModelId == query.VehicleModelId.Value ||
                p.Compatibilities.Any(c => c.VehicleModelId == query.VehicleModelId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            products = products.Where(p => p.Description != null && p.Description.Contains(query.Keyword));
        }

        if (query.Color.HasValue)
        {
            products = products.Where(p => p.Color == query.Color.Value);
        }

        products = query.SortBy switch
        {
            "priceAsc" => products.OrderBy(p => p.Price == null).ThenBy(p => p.Price),
            "priceDesc" => products.OrderBy(p => p.Price == null).ThenByDescending(p => p.Price),
            _ => products.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await products.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;

        var pageItems = await products
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Include(p => p.Category)
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(p => p.ProductImages)
            .Include(p => p.AcquisitionBatch)
            .ToListAsync(cancellationToken);

        var batchPartCounts = await GetBatchPartCountsAsync(
            pageItems.Where(p => p.AcquisitionBatchId.HasValue).Select(p => p.AcquisitionBatchId!.Value),
            cancellationToken);

        var items = pageItems.Select(p => ToDto(p, batchPartCounts)).ToList();
        if (!isAdmin)
        {
            foreach (var item in items)
            {
                item.AcquisitionCost = null;
                item.AcquisitionSource = null;
                item.AcquisitionBatchId = null;
                item.AcquisitionBatchSource = null;
                item.EffectiveAcquisitionCost = null;
                item.EffectiveAcquisitionSource = null;
            }
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(p => p.ProductImages)
            .Include(p => p.AcquisitionBatch)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var batchPartCounts = await GetBatchPartCountsAsync(
            product.AcquisitionBatchId.HasValue ? new[] { product.AcquisitionBatchId.Value } : Array.Empty<Guid>(),
            cancellationToken);

        var dto = ToDto(product, batchPartCounts);
        if (!isAdmin)
        {
            dto.AcquisitionCost = null;
            dto.AcquisitionSource = null;
            dto.AcquisitionBatchId = null;
            dto.AcquisitionBatchSource = null;
            dto.EffectiveAcquisitionCost = null;
            dto.EffectiveAcquisitionSource = null;
        }

        return dto;
    }

    public async Task<ProductResult> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Price is < 0)
        {
            return ProductResult.InvalidPrice("Price must be greater than or equal to zero when provided.");
        }

        if (request.Status is not (null or ProductStatus.Available or ProductStatus.Hidden))
        {
            return ProductResult.InvalidStatus("A product can only be created as Available or Hidden.");
        }

        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category is null)
        {
            return ProductResult.InvalidCategory("The specified category does not exist.");
        }

        var vehicleModel = await _dbContext.VehicleModels
            .Include(m => m.VehicleBrand)
            .FirstOrDefaultAsync(m => m.Id == request.SourceVehicleModelId, cancellationToken);
        if (vehicleModel is null)
        {
            return ProductResult.InvalidVehicleModel("The specified source vehicle model does not exist.");
        }

        AcquisitionBatch? acquisitionBatch = null;
        if (request.AcquisitionBatchId.HasValue)
        {
            acquisitionBatch = await _dbContext.AcquisitionBatches
                .FirstOrDefaultAsync(b => b.Id == request.AcquisitionBatchId.Value, cancellationToken);
            if (acquisitionBatch is null)
            {
                return ProductResult.InvalidAcquisitionBatch("The specified acquisition batch does not exist.");
            }
        }

        var product = new Product
        {
            CategoryId = request.CategoryId,
            SourceVehicleModelId = request.SourceVehicleModelId,
            Price = request.Price,
            AcquisitionCost = request.AcquisitionCost,
            AcquisitionSource = request.AcquisitionSource,
            AcquisitionBatchId = request.AcquisitionBatchId,
            Color = request.Color,
            Side = request.Side,
            Position = request.Position,
            Description = request.Description,
            Status = request.Status ?? ProductStatus.Available
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        product.Category = category;
        product.SourceVehicleModel = vehicleModel;
        product.AcquisitionBatch = acquisitionBatch;

        var batchPartCounts = await GetBatchPartCountsAsync(
            request.AcquisitionBatchId.HasValue ? new[] { request.AcquisitionBatchId.Value } : Array.Empty<Guid>(),
            cancellationToken);

        return ProductResult.Success(ToDto(product, batchPartCounts));
    }

    public async Task<ProductResult> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductResult.NotFound();
        }

        if (request.Price is < 0)
        {
            return ProductResult.InvalidPrice("Price must be greater than or equal to zero when provided.");
        }

        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category is null)
        {
            return ProductResult.InvalidCategory("The specified category does not exist.");
        }

        var vehicleModel = await _dbContext.VehicleModels
            .Include(m => m.VehicleBrand)
            .FirstOrDefaultAsync(m => m.Id == request.SourceVehicleModelId, cancellationToken);
        if (vehicleModel is null)
        {
            return ProductResult.InvalidVehicleModel("The specified source vehicle model does not exist.");
        }

        AcquisitionBatch? acquisitionBatch = null;
        if (request.AcquisitionBatchId.HasValue)
        {
            acquisitionBatch = await _dbContext.AcquisitionBatches
                .FirstOrDefaultAsync(b => b.Id == request.AcquisitionBatchId.Value, cancellationToken);
            if (acquisitionBatch is null)
            {
                return ProductResult.InvalidAcquisitionBatch("The specified acquisition batch does not exist.");
            }
        }

        product.CategoryId = request.CategoryId;
        product.SourceVehicleModelId = request.SourceVehicleModelId;
        product.Price = request.Price;
        product.AcquisitionCost = request.AcquisitionCost;
        product.AcquisitionSource = request.AcquisitionSource;
        product.AcquisitionBatchId = request.AcquisitionBatchId;
        product.Color = request.Color;
        product.Side = request.Side;
        product.Position = request.Position;
        product.Description = request.Description;

        await _dbContext.SaveChangesAsync(cancellationToken);

        product.Category = category;
        product.SourceVehicleModel = vehicleModel;
        product.AcquisitionBatch = acquisitionBatch;

        var batchPartCounts = await GetBatchPartCountsAsync(
            request.AcquisitionBatchId.HasValue ? new[] { request.AcquisitionBatchId.Value } : Array.Empty<Guid>(),
            cancellationToken);

        return ProductResult.Success(ToDto(product, batchPartCounts));
    }

    public async Task<ProductDeleteResult> HideProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductDeleteResult.NotFound();
        }

        product.Status = ProductStatus.Hidden;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductDeleteResult.Success();
    }

    public async Task<ProductDeleteResult> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductDeleteResult.NotFound();
        }

        if (product.Status != ProductStatus.Hidden)
        {
            return ProductDeleteResult.InvalidStatus("Only hidden products can be restored.");
        }

        product.Status = ProductStatus.Available;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductDeleteResult.Success();
    }

    public async Task<ProductResult> MarkProductSoldAsync(Guid id, MarkProductSoldRequest request, CancellationToken cancellationToken = default)
    {
        if (request.SoldPrice < 0)
        {
            return ProductResult.InvalidPrice("Sold price must be greater than or equal to zero.");
        }

        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(p => p.ProductImages)
            .Include(p => p.AcquisitionBatch)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductResult.NotFound();
        }

        if (product.Status == ProductStatus.Sold)
        {
            return ProductResult.InvalidStatus("This product is already marked as sold.");
        }

        product.Status = ProductStatus.Sold;
        product.SoldPrice = request.SoldPrice;
        product.SoldAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var batchPartCounts = await GetBatchPartCountsAsync(
            product.AcquisitionBatchId.HasValue ? new[] { product.AcquisitionBatchId.Value } : Array.Empty<Guid>(),
            cancellationToken);

        return ProductResult.Success(ToDto(product, batchPartCounts));
    }

    public async Task<ProductImageResult> AddProductImageAsync(Guid productId, UploadProductImageRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return ProductImageResult.ProductNotFound();
        }

        if (product.ProductImages.Count >= 10)
        {
            return ProductImageResult.LimitExceeded("A product cannot contain more than 10 images.");
        }

        var nextDisplayOrder = product.ProductImages.Count == 0
            ? (short)1
            : (short)(product.ProductImages.Max(i => i.DisplayOrder) + 1);

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = request.ImageUrl,
            DisplayOrder = nextDisplayOrder
        };

        _dbContext.ProductImages.Add(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductImageResult.Success(new ProductImageDto { Id = image.Id, ImageUrl = image.ImageUrl, DisplayOrder = image.DisplayOrder });
    }

    public async Task<ProductImageDeleteResult> DeleteProductImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return ProductImageDeleteResult.ProductNotFound();
        }

        var image = product.ProductImages.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return ProductImageDeleteResult.ImageNotFound();
        }

        if (product.ProductImages.Count == 1)
        {
            return ProductImageDeleteResult.MinimumRequired("A product must retain at least one image.");
        }

        _dbContext.ProductImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var remaining = product.ProductImages
            .Where(i => i.Id != imageId)
            .OrderBy(i => i.DisplayOrder)
            .ToList();

        for (var i = 0; i < remaining.Count; i++)
        {
            remaining[i].DisplayOrder = (short)(i + 1);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductImageDeleteResult.Success();
    }

    public async Task<IReadOnlyList<CompatibleVehicleModelDto>?> GetProductCompatibilityAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
        {
            return null;
        }

        return await _dbContext.ProductCompatibilities
            .Where(c => c.ProductId == productId)
            .Include(c => c.VehicleModel).ThenInclude(m => m.VehicleBrand)
            .Select(c => new CompatibleVehicleModelDto
            {
                VehicleModelId = c.VehicleModelId,
                VehicleBrandId = c.VehicleModel.VehicleBrandId,
                VehicleBrandName = c.VehicleModel.VehicleBrand.Name,
                VehicleModelName = c.VehicleModel.Name,
                StartYear = c.VehicleModel.StartYear,
                EndYear = c.VehicleModel.EndYear,
                Variant = c.VehicleModel.Variant
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductCompatibilityResult> AddProductCompatibilityAsync(Guid productId, AddProductCompatibilityRequest request, CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
        {
            return ProductCompatibilityResult.ProductNotFound();
        }

        var vehicleModel = await _dbContext.VehicleModels
            .Include(m => m.VehicleBrand)
            .FirstOrDefaultAsync(m => m.Id == request.VehicleModelId, cancellationToken);
        if (vehicleModel is null)
        {
            return ProductCompatibilityResult.VehicleModelNotFound();
        }

        var alreadyExists = await _dbContext.ProductCompatibilities
            .AnyAsync(c => c.ProductId == productId && c.VehicleModelId == request.VehicleModelId, cancellationToken);
        if (alreadyExists)
        {
            return ProductCompatibilityResult.Duplicate("This product is already marked compatible with the specified vehicle model.");
        }

        var compatibility = new ProductCompatibility
        {
            ProductId = productId,
            VehicleModelId = request.VehicleModelId
        };

        _dbContext.ProductCompatibilities.Add(compatibility);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductCompatibilityResult.Success(new CompatibleVehicleModelDto
        {
            VehicleModelId = vehicleModel.Id,
            VehicleBrandId = vehicleModel.VehicleBrandId,
            VehicleBrandName = vehicleModel.VehicleBrand.Name,
            VehicleModelName = vehicleModel.Name,
            StartYear = vehicleModel.StartYear,
            EndYear = vehicleModel.EndYear,
            Variant = vehicleModel.Variant
        });
    }

    public async Task<ProductCompatibilityDeleteResult> RemoveProductCompatibilityAsync(Guid productId, Guid vehicleModelId, CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
        {
            return ProductCompatibilityDeleteResult.ProductNotFound();
        }

        var compatibility = await _dbContext.ProductCompatibilities
            .FirstOrDefaultAsync(c => c.ProductId == productId && c.VehicleModelId == vehicleModelId, cancellationToken);
        if (compatibility is null)
        {
            return ProductCompatibilityDeleteResult.CompatibilityNotFound();
        }

        _dbContext.ProductCompatibilities.Remove(compatibility);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductCompatibilityDeleteResult.Success();
    }

    private async Task<Dictionary<Guid, int>> GetBatchPartCountsAsync(IEnumerable<Guid> batchIds, CancellationToken cancellationToken)
    {
        var ids = batchIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await _dbContext.Products
            .Where(p => p.AcquisitionBatchId.HasValue && ids.Contains(p.AcquisitionBatchId.Value))
            .GroupBy(p => p.AcquisitionBatchId!.Value)
            .Select(g => new { BatchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BatchId, x => x.Count, cancellationToken);
    }

    private static ProductDto ToDto(Product product, Dictionary<Guid, int> batchPartCounts)
    {
        var partCount = product.AcquisitionBatchId.HasValue
            ? batchPartCounts.GetValueOrDefault(product.AcquisitionBatchId.Value)
            : 0;

        var effectiveCost = product.AcquisitionCost
            ?? (product.AcquisitionBatch is not null && partCount > 0 ? product.AcquisitionBatch.TotalCost / partCount : null);

        var effectiveSource = product.AcquisitionSource ?? product.AcquisitionBatch?.Source;

        var dto = ToDto(product);
        dto.AcquisitionBatchId = product.AcquisitionBatchId;
        dto.AcquisitionBatchSource = product.AcquisitionBatch?.Source;
        dto.EffectiveAcquisitionCost = effectiveCost;
        dto.EffectiveAcquisitionSource = effectiveSource;
        return dto;
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
        SoldPrice = product.SoldPrice,
        AcquisitionCost = product.AcquisitionCost,
        AcquisitionSource = product.AcquisitionSource,
        Color = product.Color,
        Status = product.Status,
        Side = product.Side,
        Position = product.Position,
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
