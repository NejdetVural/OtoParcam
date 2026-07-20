using OtoParcam.Domain.Common; 

namespace OtoParcam.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}