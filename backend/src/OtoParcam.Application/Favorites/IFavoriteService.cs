using OtoParcam.Application.Products;

namespace OtoParcam.Application.Favorites;

public interface IFavoriteService
{
    Task<IReadOnlyList<ProductDto>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FavoriteResult> AddFavoriteAsync(Guid userId, AddFavoriteRequest request, CancellationToken cancellationToken = default);
    Task<FavoriteDeleteResult> RemoveFavoriteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
