using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.PurchaseRequests;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/admin/purchase-requests")]
[Authorize(Roles = Roles.Administrator)]
public class AdminPurchaseRequestsController : ControllerBase
{
    private readonly IPurchaseRequestService _purchaseRequestService;

    public AdminPurchaseRequestsController(IPurchaseRequestService purchaseRequestService)
    {
        _purchaseRequestService = purchaseRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPurchaseRequests(CancellationToken cancellationToken)
    {
        var purchaseRequests = await _purchaseRequestService.GetAllPurchaseRequestsAsync(cancellationToken);
        return Ok(purchaseRequests);
    }

    [HttpPatch("{id:guid}/negotiation")]
    public async Task<IActionResult> UpdateNegotiation(Guid id, UpdateNegotiationRequest request, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.UpdateNegotiationAsync(id, request, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => Ok(result.PurchaseRequest),
            PurchaseRequestOperationStatus.NotFound => NotFound(),
            PurchaseRequestOperationStatus.ProductNotFound => BadRequest(new { error = result.Error }),
            PurchaseRequestOperationStatus.InvalidPrice => BadRequest(new { error = result.Error }),
            PurchaseRequestOperationStatus.InvalidTransition => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }
}
