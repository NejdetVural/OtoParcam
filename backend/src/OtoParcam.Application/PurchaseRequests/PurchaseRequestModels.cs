using System.ComponentModel.DataAnnotations;
using OtoParcam.Domain.Enums;

namespace OtoParcam.Application.PurchaseRequests;

public class PurchaseRequestItemDto
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public decimal? OriginalPrice { get; set; }
    public decimal? NegotiatedPrice { get; set; }
}

public class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public PurchaseRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<PurchaseRequestItemDto> Items { get; set; } = Array.Empty<PurchaseRequestItemDto>();
}

public class CreatePurchaseRequestRequest
{
    [Required, MinLength(1)]
    public List<Guid> ProductIds { get; set; } = new();
}

public class NegotiationItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public decimal NegotiatedPrice { get; set; }
}

public class UpdateNegotiationRequest
{
    [Required, MinLength(1)]
    public List<NegotiationItemRequest> Items { get; set; } = new();
}

public enum PurchaseRequestOperationStatus
{
    Success,
    NotFound,
    EmptyProductIds,
    ProductNotFound,
    ProductNotAvailable,
    ProductAlreadyRequested,
    InvalidTransition,
    InvalidPrice
}

public class PurchaseRequestResult
{
    public PurchaseRequestOperationStatus Status { get; init; }
    public PurchaseRequestDto? PurchaseRequest { get; init; }
    public string? Error { get; init; }

    public static PurchaseRequestResult Success(PurchaseRequestDto purchaseRequest) =>
        new() { Status = PurchaseRequestOperationStatus.Success, PurchaseRequest = purchaseRequest };
    public static PurchaseRequestResult NotFound() =>
        new() { Status = PurchaseRequestOperationStatus.NotFound };
    public static PurchaseRequestResult EmptyProductIds(string error) =>
        new() { Status = PurchaseRequestOperationStatus.EmptyProductIds, Error = error };
    public static PurchaseRequestResult ProductNotFound(string error) =>
        new() { Status = PurchaseRequestOperationStatus.ProductNotFound, Error = error };
    public static PurchaseRequestResult ProductNotAvailable(string error) =>
        new() { Status = PurchaseRequestOperationStatus.ProductNotAvailable, Error = error };
    public static PurchaseRequestResult ProductAlreadyRequested(string error) =>
        new() { Status = PurchaseRequestOperationStatus.ProductAlreadyRequested, Error = error };
    public static PurchaseRequestResult InvalidTransition(string error) =>
        new() { Status = PurchaseRequestOperationStatus.InvalidTransition, Error = error };
    public static PurchaseRequestResult InvalidPrice(string error) =>
        new() { Status = PurchaseRequestOperationStatus.InvalidPrice, Error = error };
}
