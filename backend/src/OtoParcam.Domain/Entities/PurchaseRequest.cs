using OtoParcam.Domain.Common;
using OtoParcam.Domain.Enums;

namespace OtoParcam.Domain.Entities;

public class PurchaseRequest : BaseEntity
{
	public Guid ApplicationUserId { get; set; }
	public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Pending;

	// Navigation properties
	public ApplicationUser ApplicationUser { get; set; } = null!;
	public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
}