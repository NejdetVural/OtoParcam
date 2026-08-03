using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.PurchaseRequests;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/purchase-requests")]
[Authorize(Roles = Roles.Customer)]
public class PurchaseRequestsController : ControllerBase
{
    private readonly IPurchaseRequestService _purchaseRequestService;

    public PurchaseRequestsController(IPurchaseRequestService purchaseRequestService)
    {
        _purchaseRequestService = purchaseRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchaseRequests(CancellationToken cancellationToken)
    {
        var purchaseRequests = await _purchaseRequestService.GetPurchaseRequestsAsync(GetUserId(), cancellationToken);
        return Ok(purchaseRequests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPurchaseRequestById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.GetPurchaseRequestByIdAsync(GetUserId(), id, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => Ok(result.PurchaseRequest),
            PurchaseRequestOperationStatus.NotFound => NotFound(),
            _ => BadRequest()
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseRequest(CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.CreatePurchaseRequestAsync(GetUserId(), request, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.PurchaseRequest),
            PurchaseRequestOperationStatus.EmptyProductIds => BadRequest(new { error = result.Error }),
            PurchaseRequestOperationStatus.ProductNotFound => BadRequest(new { error = result.Error }),
            PurchaseRequestOperationStatus.ProductNotAvailable => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelPurchaseRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.CancelPurchaseRequestAsync(GetUserId(), id, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => Ok(result.PurchaseRequest),
            PurchaseRequestOperationStatus.NotFound => NotFound(),
            PurchaseRequestOperationStatus.InvalidTransition => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmPurchaseRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.ConfirmPurchaseRequestAsync(GetUserId(), id, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => Ok(result.PurchaseRequest),
            PurchaseRequestOperationStatus.NotFound => NotFound(),
            PurchaseRequestOperationStatus.InvalidTransition => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> RejectPurchaseRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.RejectPurchaseRequestAsync(GetUserId(), id, cancellationToken);
        return result.Status switch
        {
            PurchaseRequestOperationStatus.Success => Ok(result.PurchaseRequest),
            PurchaseRequestOperationStatus.NotFound => NotFound(),
            PurchaseRequestOperationStatus.InvalidTransition => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
