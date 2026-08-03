using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.PurchaseRequests;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly ApplicationDbContext _dbContext;

    public PurchaseRequestService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PurchaseRequestDto>> GetPurchaseRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var purchaseRequests = await Query()
            .Where(r => r.ApplicationUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return purchaseRequests.Select(ToDto).ToList();
    }

    public async Task<PurchaseRequestResult> GetPurchaseRequestByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (purchaseRequest is null || purchaseRequest.ApplicationUserId != userId)
        {
            return PurchaseRequestResult.NotFound();
        }

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    public async Task<PurchaseRequestResult> CreatePurchaseRequestAsync(Guid userId, CreatePurchaseRequestRequest request, CancellationToken cancellationToken = default)
    {
        var productIds = request.ProductIds.Distinct().ToList();
        if (productIds.Count == 0)
        {
            return PurchaseRequestResult.EmptyProductIds("A purchase request must contain at least one product.");
        }

        var products = await _dbContext.Products
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return PurchaseRequestResult.ProductNotFound("One or more specified products do not exist.");
        }

        if (products.Any(p => p.Status != ProductStatus.Available))
        {
            return PurchaseRequestResult.ProductNotAvailable("One or more specified products are not currently in stock.");
        }

        var user = await _dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var purchaseRequest = new PurchaseRequest
        {
            ApplicationUserId = userId,
            Status = PurchaseRequestStatus.Pending
        };

        foreach (var product in products)
        {
            purchaseRequest.Items.Add(new PurchaseRequestItem
            {
                ProductId = product.Id,
                OriginalPrice = product.Price,
                NegotiatedPrice = null
            });
        }

        _dbContext.PurchaseRequests.Add(purchaseRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        purchaseRequest.ApplicationUser = user;
        foreach (var item in purchaseRequest.Items)
        {
            item.Product = products.First(p => p.Id == item.ProductId);
        }

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    public async Task<PurchaseRequestResult> CancelPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (purchaseRequest is null || purchaseRequest.ApplicationUserId != userId)
        {
            return PurchaseRequestResult.NotFound();
        }

        if (purchaseRequest.Status != PurchaseRequestStatus.Pending)
        {
            return PurchaseRequestResult.InvalidTransition("Only pending purchase requests can be cancelled.");
        }

        purchaseRequest.Status = PurchaseRequestStatus.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    public async Task<PurchaseRequestResult> ConfirmPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (purchaseRequest is null || purchaseRequest.ApplicationUserId != userId)
        {
            return PurchaseRequestResult.NotFound();
        }

        if (purchaseRequest.Status is not (PurchaseRequestStatus.Pending or PurchaseRequestStatus.WaitingForCustomerConfirmation))
        {
            return PurchaseRequestResult.InvalidTransition("Only pending or awaiting-confirmation purchase requests can be confirmed.");
        }

        purchaseRequest.Status = PurchaseRequestStatus.Approved;
        foreach (var item in purchaseRequest.Items)
        {
            item.Product.Status = ProductStatus.Sold;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    public async Task<PurchaseRequestResult> RejectPurchaseRequestAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (purchaseRequest is null || purchaseRequest.ApplicationUserId != userId)
        {
            return PurchaseRequestResult.NotFound();
        }

        if (purchaseRequest.Status is not (PurchaseRequestStatus.Pending or PurchaseRequestStatus.WaitingForCustomerConfirmation))
        {
            return PurchaseRequestResult.InvalidTransition("Only pending or awaiting-confirmation purchase requests can be rejected.");
        }

        purchaseRequest.Status = PurchaseRequestStatus.Rejected;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    public async Task<IReadOnlyList<PurchaseRequestDto>> GetAllPurchaseRequestsAsync(CancellationToken cancellationToken = default)
    {
        var purchaseRequests = await Query()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return purchaseRequests.Select(ToDto).ToList();
    }

    public async Task<PurchaseRequestResult> UpdateNegotiationAsync(Guid id, UpdateNegotiationRequest request, CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (purchaseRequest is null)
        {
            return PurchaseRequestResult.NotFound();
        }

        if (purchaseRequest.Status is not (PurchaseRequestStatus.Pending or PurchaseRequestStatus.WaitingForCustomerConfirmation))
        {
            return PurchaseRequestResult.InvalidTransition("Negotiated prices can only be updated while the request is pending or awaiting confirmation.");
        }

        if (request.Items.Any(i => i.NegotiatedPrice < 0))
        {
            return PurchaseRequestResult.InvalidPrice("Negotiated price must be greater than or equal to zero.");
        }

        foreach (var negotiationItem in request.Items)
        {
            var item = purchaseRequest.Items.FirstOrDefault(i => i.ProductId == negotiationItem.ProductId);
            if (item is null)
            {
                return PurchaseRequestResult.ProductNotFound($"Product {negotiationItem.ProductId} is not part of this purchase request.");
            }

            item.NegotiatedPrice = negotiationItem.NegotiatedPrice;
        }

        purchaseRequest.Status = PurchaseRequestStatus.WaitingForCustomerConfirmation;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PurchaseRequestResult.Success(ToDto(purchaseRequest));
    }

    private IQueryable<PurchaseRequest> Query() => _dbContext.PurchaseRequests
        .Include(r => r.ApplicationUser)
        .Include(r => r.Items).ThenInclude(i => i.Product).ThenInclude(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand);

    private static PurchaseRequestDto ToDto(PurchaseRequest purchaseRequest) => new()
    {
        Id = purchaseRequest.Id,
        ApplicationUserId = purchaseRequest.ApplicationUserId,
        CustomerName = $"{purchaseRequest.ApplicationUser.FirstName} {purchaseRequest.ApplicationUser.LastName}",
        CustomerEmail = purchaseRequest.ApplicationUser.Email ?? string.Empty,
        Status = purchaseRequest.Status,
        CreatedAt = purchaseRequest.CreatedAt,
        Items = purchaseRequest.Items
            .Select(i => new PurchaseRequestItemDto
            {
                ProductId = i.ProductId,
                ProductTitle = BuildTitle(i.Product.SourceVehicleModel),
                OriginalPrice = i.OriginalPrice,
                NegotiatedPrice = i.NegotiatedPrice
            })
            .ToList()
    };

    private static string BuildTitle(VehicleModel vehicleModel)
    {
        var variantPart = string.IsNullOrWhiteSpace(vehicleModel.Variant) ? string.Empty : $" {vehicleModel.Variant}";
        return $"{vehicleModel.VehicleBrand.Name} {vehicleModel.Name}{variantPart} ({vehicleModel.StartYear}-{vehicleModel.EndYear})";
    }
}
