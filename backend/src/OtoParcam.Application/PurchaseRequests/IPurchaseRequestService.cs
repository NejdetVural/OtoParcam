namespace OtoParcam.Application.PurchaseRequests;

public interface IPurchaseRequestService
{
    Task<IReadOnlyList<PurchaseRequestDto>> GetPurchaseRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> GetPurchaseRequestByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> CreatePurchaseRequestAsync(Guid userId, CreatePurchaseRequestRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> CancelPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> ConfirmPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> RejectPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseRequestDto>> GetAllPurchaseRequestsAsync(CancellationToken cancellationToken = default);
    Task<PurchaseRequestResult> UpdateNegotiationAsync(Guid id, UpdateNegotiationRequest request, CancellationToken cancellationToken = default);
}
