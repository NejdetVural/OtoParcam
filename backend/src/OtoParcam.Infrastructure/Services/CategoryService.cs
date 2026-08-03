using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Categories;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResult> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var nameExists = await _dbContext.Categories.AnyAsync(c => c.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            return CategoryResult.Conflict("A category with this name already exists.");
        }

        var category = new Category { Name = request.Name };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CategoryResult.Success(new CategoryDto { Id = category.Id, Name = category.Name });
    }

    public async Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return CategoryResult.NotFound();
        }

        var nameTaken = await _dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return CategoryResult.Conflict("A category with this name already exists.");
        }

        category.Name = request.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CategoryResult.Success(new CategoryDto { Id = category.Id, Name = category.Name });
    }

    public async Task<CategoryDeleteResult> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return CategoryDeleteResult.NotFound();
        }

        var hasProducts = await _dbContext.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            return CategoryDeleteResult.Conflict("Category cannot be deleted while referenced by one or more products.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CategoryDeleteResult.Success();
    }
}
