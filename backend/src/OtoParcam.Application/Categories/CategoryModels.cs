using System.ComponentModel.DataAnnotations;

namespace OtoParcam.Application.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public enum CategoryOperationStatus
{
    Success,
    NotFound,
    Conflict
}

public class CategoryResult
{
    public CategoryOperationStatus Status { get; init; }
    public CategoryDto? Category { get; init; }
    public string? Error { get; init; }

    public static CategoryResult Success(CategoryDto category) => new() { Status = CategoryOperationStatus.Success, Category = category };
    public static CategoryResult NotFound() => new() { Status = CategoryOperationStatus.NotFound };
    public static CategoryResult Conflict(string error) => new() { Status = CategoryOperationStatus.Conflict, Error = error };
}

public class CategoryDeleteResult
{
    public CategoryOperationStatus Status { get; init; }
    public string? Error { get; init; }

    public static CategoryDeleteResult Success() => new() { Status = CategoryOperationStatus.Success };
    public static CategoryDeleteResult NotFound() => new() { Status = CategoryOperationStatus.NotFound };
    public static CategoryDeleteResult Conflict(string error) => new() { Status = CategoryOperationStatus.Conflict, Error = error };
}
