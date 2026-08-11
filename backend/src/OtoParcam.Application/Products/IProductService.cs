namespace OtoParcam.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductListQuery query, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(Guid id, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ProductResult> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResult> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDeleteResult> HideProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDeleteResult> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResult> MarkProductSoldAsync(Guid id, MarkProductSoldRequest request, CancellationToken cancellationToken = default);
    Task<ProductImageResult> AddProductImageAsync(Guid productId, UploadProductImageRequest request, CancellationToken cancellationToken = default);
    Task<ProductImageDeleteResult> DeleteProductImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompatibleVehicleModelDto>?> GetProductCompatibilityAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductCompatibilityResult> AddProductCompatibilityAsync(Guid productId, AddProductCompatibilityRequest request, CancellationToken cancellationToken = default);
    Task<ProductCompatibilityDeleteResult> RemoveProductCompatibilityAsync(Guid productId, Guid vehicleModelId, CancellationToken cancellationToken = default);
}
