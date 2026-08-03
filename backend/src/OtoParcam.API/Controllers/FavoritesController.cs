using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Favorites;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/favorites")]
[Authorize(Roles = Roles.Customer)]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites(CancellationToken cancellationToken)
    {
        var favorites = await _favoriteService.GetFavoritesAsync(GetUserId(), cancellationToken);
        return Ok(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite(AddFavoriteRequest request, CancellationToken cancellationToken)
    {
        var result = await _favoriteService.AddFavoriteAsync(GetUserId(), request, cancellationToken);
        return result.Status switch
        {
            FavoriteOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.Product),
            FavoriteOperationStatus.ProductNotFound => NotFound(),
            FavoriteOperationStatus.Duplicate => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFavorite(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _favoriteService.RemoveFavoriteAsync(GetUserId(), productId, cancellationToken);
        return result.Status switch
        {
            FavoriteOperationStatus.Success => NoContent(),
            FavoriteOperationStatus.NotFound => NotFound(),
            _ => BadRequest()
        };
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
