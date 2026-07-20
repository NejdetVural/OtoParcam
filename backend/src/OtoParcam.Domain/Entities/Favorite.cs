using OtoParcam.Domain.Common;

namespace OtoParcam.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid ApplicationUserId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation properties
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Product Product { get; set; } = null!;
}