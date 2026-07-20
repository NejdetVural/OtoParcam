using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public short DisplayOrder { get; set; }

    // Navigation property
    public Product Product { get; set; } = null!;
}