using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Dashboard;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalProducts = await _dbContext.Products.CountAsync(cancellationToken);

        var customerRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Customer)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var totalCustomers = await _dbContext.UserRoles
            .CountAsync(ur => ur.RoleId == customerRoleId, cancellationToken);

        var pendingPurchaseRequests = await _dbContext.PurchaseRequests
            .CountAsync(r => r.Status == PurchaseRequestStatus.Pending, cancellationToken);

        var productsAwaitingAttention = await _dbContext.Products
            .Where(p => p.Status == ProductStatus.Available
                && (!p.ProductImages.Any() || !p.Compatibilities.Any()))
            .CountAsync(cancellationToken);

        var acquisitionBatchesInProgress = await _dbContext.AcquisitionBatches
            .CountAsync(b => b.Products.Any(p => p.Status == ProductStatus.Available), cancellationToken);

        return new DashboardStatsDto
        {
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            PendingPurchaseRequests = pendingPurchaseRequests,
            ProductsAwaitingAttention = productsAwaitingAttention,
            AcquisitionBatchesInProgress = acquisitionBatchesInProgress
        };
    }
}
