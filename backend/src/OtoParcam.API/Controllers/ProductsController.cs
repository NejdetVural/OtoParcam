using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.API.Services;
using OtoParcam.Application.Products;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductImageFileStorage _imageStorage;

    public ProductsController(IProductService productService, IProductImageFileStorage imageStorage)
    {
        _productService = productService;
        _imageStorage = imageStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductListQuery query, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Roles.Administrator);
        var result = await _productService.GetProductsAsync(query, isAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(Roles.Administrator);
        var product = await _productService.GetProductByIdAsync(id, isAdmin, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateProductAsync(request, cancellationToken);
        return result.Status switch
        {
            ProductOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.Product),
            ProductOperationStatus.InvalidCategory => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidVehicleModel => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidPrice => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidAcquisitionBatch => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidStatus => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateProductAsync(id, request, cancellationToken);
        return result.Status switch
        {
            ProductOperationStatus.Success => NoContent(),
            ProductOperationStatus.NotFound => NotFound(),
            ProductOperationStatus.InvalidCategory => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidVehicleModel => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidPrice => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidAcquisitionBatch => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> HideProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.HideProductAsync(id, cancellationToken);
        return result.Status switch
        {
            ProductOperationStatus.Success => NoContent(),
            ProductOperationStatus.NotFound => NotFound(),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> RestoreProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.RestoreProductAsync(id, cancellationToken);
        return result.Status switch
        {
            ProductOperationStatus.Success => NoContent(),
            ProductOperationStatus.NotFound => NotFound(),
            ProductOperationStatus.InvalidStatus => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/sell")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> MarkProductSold(Guid id, MarkProductSoldRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.MarkProductSoldAsync(id, request, cancellationToken);
        return result.Status switch
        {
            ProductOperationStatus.Success => Ok(result.Product),
            ProductOperationStatus.NotFound => NotFound(),
            ProductOperationStatus.InvalidPrice => BadRequest(new { error = result.Error }),
            ProductOperationStatus.InvalidStatus => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = Roles.Administrator)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> AddProductImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "An image file is required." });
        }

        string relativeUrl;
        try
        {
            relativeUrl = await _imageStorage.SaveAsync(id, file, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var result = await _productService.AddProductImageAsync(id, new UploadProductImageRequest { ImageUrl = relativeUrl }, cancellationToken);

        if (result.Status != ProductImageOperationStatus.Success)
        {
            _imageStorage.Delete(relativeUrl);
        }

        return result.Status switch
        {
            ProductImageOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.Image),
            ProductImageOperationStatus.ProductNotFound => NotFound(),
            ProductImageOperationStatus.LimitExceeded => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteProductImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, isAdmin: true, cancellationToken);
        var imageUrl = product?.Images.FirstOrDefault(i => i.Id == imageId)?.ImageUrl;

        var result = await _productService.DeleteProductImageAsync(id, imageId, cancellationToken);

        if (result.Status == ProductImageOperationStatus.Success && imageUrl is not null)
        {
            _imageStorage.Delete(imageUrl);
        }

        return result.Status switch
        {
            ProductImageOperationStatus.Success => NoContent(),
            ProductImageOperationStatus.ProductNotFound => NotFound(),
            ProductImageOperationStatus.ImageNotFound => NotFound(),
            ProductImageOperationStatus.MinimumRequired => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpGet("{id:guid}/compatibility")]
    public async Task<IActionResult> GetProductCompatibility(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductCompatibilityAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/compatibility")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> AddProductCompatibility(Guid id, AddProductCompatibilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.AddProductCompatibilityAsync(id, request, cancellationToken);
        return result.Status switch
        {
            ProductCompatibilityOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.VehicleModel),
            ProductCompatibilityOperationStatus.ProductNotFound => NotFound(),
            ProductCompatibilityOperationStatus.VehicleModelNotFound => BadRequest(new { error = "The specified vehicle model does not exist." }),
            ProductCompatibilityOperationStatus.Duplicate => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}/compatibility/{vehicleModelId:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> RemoveProductCompatibility(Guid id, Guid vehicleModelId, CancellationToken cancellationToken)
    {
        var result = await _productService.RemoveProductCompatibilityAsync(id, vehicleModelId, cancellationToken);
        return result.Status switch
        {
            ProductCompatibilityOperationStatus.Success => NoContent(),
            ProductCompatibilityOperationStatus.ProductNotFound => NotFound(),
            ProductCompatibilityOperationStatus.CompatibilityNotFound => NotFound(),
            _ => BadRequest()
        };
    }
}
