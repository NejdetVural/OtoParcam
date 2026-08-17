using OtoParcam.Application.Categories;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories_OrderedAlphabetically()
    {
        await using var context = CreateContext();
        context.Categories.AddRange(
            new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Motor" },
            new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Fren" },
            new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Kaporta" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.GetCategoriesAsync();

        Assert.Equal(new[] { "Fren", "Kaporta", "Motor" }, result.Select(c => c.Name));
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateName_ReturnsConflict()
    {
        await using var context = CreateContext();
        context.Categories.Add(new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Motor" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.CreateCategoryAsync(new CreateCategoryRequest { Name = "Motor" });

        Assert.Equal(CategoryOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CreateCategoryAsync_NewName_ReturnsSuccessWithGeneratedId()
    {
        await using var context = CreateContext();
        var service = new CategoryService(context);

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest { Name = "Motor" });

        Assert.Equal(CategoryOperationStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.Category!.Id);
        Assert.Equal("Motor", result.Category.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new CategoryService(context);

        var result = await service.UpdateCategoryAsync(Guid.NewGuid(), new UpdateCategoryRequest { Name = "Motor" });

        Assert.Equal(CategoryOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateCategoryAsync_RenameToAnotherCategorysName_ReturnsConflict()
    {
        await using var context = CreateContext();
        var target = new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Motor" };
        context.Categories.AddRange(target, new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Fren" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.UpdateCategoryAsync(target.Id, new UpdateCategoryRequest { Name = "Fren" });

        Assert.Equal(CategoryOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task UpdateCategoryAsync_RenameToOwnUnchangedName_DoesNotConflict()
    {
        await using var context = CreateContext();
        var target = new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Motor" };
        context.Categories.Add(target);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.UpdateCategoryAsync(target.Id, new UpdateCategoryRequest { Name = "Motor" });

        Assert.Equal(CategoryOperationStatus.Success, result.Status);
    }

    [Fact]
    public async Task DeleteCategoryAsync_UnknownId_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var service = new CategoryService(context);

        var result = await service.DeleteCategoryAsync(Guid.NewGuid());

        Assert.Equal(CategoryOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReferencedByProduct_ReturnsConflict()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, Domain.Enums.ProductStatus.Available));
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.DeleteCategoryAsync(category.Id);

        Assert.Equal(CategoryOperationStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteCategoryAsync_NoReferences_RemovesCategory()
    {
        await using var context = CreateContext();
        var category = new Domain.Entities.Category { Id = Guid.NewGuid(), Name = "Motor" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = await service.DeleteCategoryAsync(category.Id);

        Assert.Equal(CategoryOperationStatus.Success, result.Status);
        Assert.Null(await context.Categories.FindAsync(category.Id));
    }
}
