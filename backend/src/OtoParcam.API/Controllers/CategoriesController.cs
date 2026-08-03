using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Categories;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return result.Status switch
        {
            CategoryOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.Category),
            CategoryOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return result.Status switch
        {
            CategoryOperationStatus.Success => NoContent(),
            CategoryOperationStatus.NotFound => NotFound(),
            CategoryOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return result.Status switch
        {
            CategoryOperationStatus.Success => NoContent(),
            CategoryOperationStatus.NotFound => NotFound(),
            CategoryOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }
}
