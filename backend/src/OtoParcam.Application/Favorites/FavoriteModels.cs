using System.ComponentModel.DataAnnotations;
using OtoParcam.Application.Products;

namespace OtoParcam.Application.Favorites;

public class AddFavoriteRequest
{
    [Required]
    public Guid ProductId { get; set; }
}

public enum FavoriteOperationStatus
{
    Success,
    ProductNotFound,
    Duplicate,
    NotFound
}

public class FavoriteResult
{
    public FavoriteOperationStatus Status { get; init; }
    public ProductDto? Product { get; init; }
    public string? Error { get; init; }

    public static FavoriteResult Success(ProductDto product) => new() { Status = FavoriteOperationStatus.Success, Product = product };
    public static FavoriteResult ProductNotFound() => new() { Status = FavoriteOperationStatus.ProductNotFound };
    public static FavoriteResult Duplicate(string error) => new() { Status = FavoriteOperationStatus.Duplicate, Error = error };
}

public class FavoriteDeleteResult
{
    public FavoriteOperationStatus Status { get; init; }

    public static FavoriteDeleteResult Success() => new() { Status = FavoriteOperationStatus.Success };
    public static FavoriteDeleteResult NotFound() => new() { Status = FavoriteOperationStatus.NotFound };
}
