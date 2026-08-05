using System.ComponentModel.DataAnnotations;
using OtoParcam.Domain.Enums;

namespace OtoParcam.Application.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid SourceVehicleModelId { get; set; }
    public Guid VehicleBrandId { get; set; }
    public string VehicleBrandName { get; set; } = string.Empty;
    public string VehicleModelName { get; set; } = string.Empty;
    public short StartYear { get; set; }
    public short EndYear { get; set; }
    public string? Variant { get; set; }
    public decimal? Price { get; set; }
    public ProductColor Color { get; set; }
    public ProductStatus Status { get; set; }
    public ProductSide? Side { get; set; }
    public ProductPosition? Position { get; set; }
    public string? Description { get; set; }
    public IReadOnlyList<ProductImageDto> Images { get; set; } = Array.Empty<ProductImageDto>();
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public short DisplayOrder { get; set; }
}

public class UploadProductImageRequest
{
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}

public class ProductListQuery
{
    public Guid? CategoryId { get; set; }
    public Guid? VehicleBrandId { get; set; }
    public Guid? VehicleModelId { get; set; }
    public string? Keyword { get; set; }
    public ProductColor? Color { get; set; }
    public int Page { get; set; } = 1;
    public string? SortBy { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class CreateProductRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid SourceVehicleModelId { get; set; }

    public decimal? Price { get; set; }

    [Required]
    public ProductColor Color { get; set; }

    public ProductSide? Side { get; set; }

    public ProductPosition? Position { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class UpdateProductRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid SourceVehicleModelId { get; set; }

    public decimal? Price { get; set; }

    [Required]
    public ProductColor Color { get; set; }

    public ProductSide? Side { get; set; }

    public ProductPosition? Position { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public enum ProductOperationStatus
{
    Success,
    NotFound,
    InvalidCategory,
    InvalidVehicleModel,
    InvalidPrice
}

public class ProductResult
{
    public ProductOperationStatus Status { get; init; }
    public ProductDto? Product { get; init; }
    public string? Error { get; init; }

    public static ProductResult Success(ProductDto product) => new() { Status = ProductOperationStatus.Success, Product = product };
    public static ProductResult NotFound() => new() { Status = ProductOperationStatus.NotFound };
    public static ProductResult InvalidCategory(string error) => new() { Status = ProductOperationStatus.InvalidCategory, Error = error };
    public static ProductResult InvalidVehicleModel(string error) => new() { Status = ProductOperationStatus.InvalidVehicleModel, Error = error };
    public static ProductResult InvalidPrice(string error) => new() { Status = ProductOperationStatus.InvalidPrice, Error = error };
}

public class ProductDeleteResult
{
    public ProductOperationStatus Status { get; init; }

    public static ProductDeleteResult Success() => new() { Status = ProductOperationStatus.Success };
    public static ProductDeleteResult NotFound() => new() { Status = ProductOperationStatus.NotFound };
}

public enum ProductImageOperationStatus
{
    Success,
    ProductNotFound,
    ImageNotFound,
    LimitExceeded,
    MinimumRequired
}

public class ProductImageResult
{
    public ProductImageOperationStatus Status { get; init; }
    public ProductImageDto? Image { get; init; }
    public string? Error { get; init; }

    public static ProductImageResult Success(ProductImageDto image) => new() { Status = ProductImageOperationStatus.Success, Image = image };
    public static ProductImageResult ProductNotFound() => new() { Status = ProductImageOperationStatus.ProductNotFound };
    public static ProductImageResult LimitExceeded(string error) => new() { Status = ProductImageOperationStatus.LimitExceeded, Error = error };
}

public class ProductImageDeleteResult
{
    public ProductImageOperationStatus Status { get; init; }
    public string? Error { get; init; }

    public static ProductImageDeleteResult Success() => new() { Status = ProductImageOperationStatus.Success };
    public static ProductImageDeleteResult ProductNotFound() => new() { Status = ProductImageOperationStatus.ProductNotFound };
    public static ProductImageDeleteResult ImageNotFound() => new() { Status = ProductImageOperationStatus.ImageNotFound };
    public static ProductImageDeleteResult MinimumRequired(string error) => new() { Status = ProductImageOperationStatus.MinimumRequired, Error = error };
}

public class CompatibleVehicleModelDto
{
    public Guid VehicleModelId { get; set; }
    public Guid VehicleBrandId { get; set; }
    public string VehicleBrandName { get; set; } = string.Empty;
    public string VehicleModelName { get; set; } = string.Empty;
    public short StartYear { get; set; }
    public short EndYear { get; set; }
    public string? Variant { get; set; }
}

public class AddProductCompatibilityRequest
{
    [Required]
    public Guid VehicleModelId { get; set; }
}

public enum ProductCompatibilityOperationStatus
{
    Success,
    ProductNotFound,
    VehicleModelNotFound,
    Duplicate,
    CompatibilityNotFound
}

public class ProductCompatibilityResult
{
    public ProductCompatibilityOperationStatus Status { get; init; }
    public CompatibleVehicleModelDto? VehicleModel { get; init; }
    public string? Error { get; init; }

    public static ProductCompatibilityResult Success(CompatibleVehicleModelDto vehicleModel) => new() { Status = ProductCompatibilityOperationStatus.Success, VehicleModel = vehicleModel };
    public static ProductCompatibilityResult ProductNotFound() => new() { Status = ProductCompatibilityOperationStatus.ProductNotFound };
    public static ProductCompatibilityResult VehicleModelNotFound() => new() { Status = ProductCompatibilityOperationStatus.VehicleModelNotFound };
    public static ProductCompatibilityResult Duplicate(string error) => new() { Status = ProductCompatibilityOperationStatus.Duplicate, Error = error };
}

public class ProductCompatibilityDeleteResult
{
    public ProductCompatibilityOperationStatus Status { get; init; }

    public static ProductCompatibilityDeleteResult Success() => new() { Status = ProductCompatibilityOperationStatus.Success };
    public static ProductCompatibilityDeleteResult ProductNotFound() => new() { Status = ProductCompatibilityOperationStatus.ProductNotFound };
    public static ProductCompatibilityDeleteResult CompatibilityNotFound() => new() { Status = ProductCompatibilityOperationStatus.CompatibilityNotFound };
}
