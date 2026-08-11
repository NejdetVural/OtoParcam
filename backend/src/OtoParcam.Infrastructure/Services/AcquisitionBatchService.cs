using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.AcquisitionBatches;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class AcquisitionBatchService : IAcquisitionBatchService
{
    private readonly ApplicationDbContext _dbContext;

    public AcquisitionBatchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AcquisitionBatchDto>> GetBatchesAsync(CancellationToken cancellationToken = default)
    {
        var batches = await _dbContext.AcquisitionBatches
            .OrderByDescending(b => b.PurchaseDate)
            .ToListAsync(cancellationToken);

        if (batches.Count == 0)
        {
            return Array.Empty<AcquisitionBatchDto>();
        }

        var batchIds = batches.Select(b => b.Id).ToList();
        var products = await GetProductsForBatchesAsync(batchIds, cancellationToken);

        return batches
            .Select(b => BuildDto(b, products.Where(p => p.AcquisitionBatchId == b.Id)))
            .ToList();
    }

    public async Task<AcquisitionBatchDto?> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.AcquisitionBatches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (batch is null)
        {
            return null;
        }

        var products = await GetProductsForBatchesAsync(new List<Guid> { id }, cancellationToken);
        return BuildDto(batch, products);
    }

    public async Task<AcquisitionBatchResult> CreateBatchAsync(CreateAcquisitionBatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TotalCost < 0)
        {
            return AcquisitionBatchResult.InvalidCost("Total cost must be greater than or equal to zero.");
        }

        var batch = new AcquisitionBatch
        {
            Source = request.Source,
            TotalCost = request.TotalCost,
            PurchaseDate = request.PurchaseDate,
            Notes = request.Notes
        };

        _dbContext.AcquisitionBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AcquisitionBatchResult.Success(BuildDto(batch, Enumerable.Empty<Product>()));
    }

    public async Task<AcquisitionBatchResult> UpdateBatchAsync(Guid id, UpdateAcquisitionBatchRequest request, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.AcquisitionBatches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (batch is null)
        {
            return AcquisitionBatchResult.NotFound();
        }

        if (request.TotalCost < 0)
        {
            return AcquisitionBatchResult.InvalidCost("Total cost must be greater than or equal to zero.");
        }

        batch.Source = request.Source;
        batch.TotalCost = request.TotalCost;
        batch.PurchaseDate = request.PurchaseDate;
        batch.Notes = request.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var products = await GetProductsForBatchesAsync(new List<Guid> { id }, cancellationToken);
        return AcquisitionBatchResult.Success(BuildDto(batch, products));
    }

    public async Task<AcquisitionBatchDeleteResult> DeleteBatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.AcquisitionBatches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (batch is null)
        {
            return AcquisitionBatchDeleteResult.NotFound();
        }

        var hasProducts = await _dbContext.Products.AnyAsync(p => p.AcquisitionBatchId == id, cancellationToken);
        if (hasProducts)
        {
            return AcquisitionBatchDeleteResult.Conflict("Acquisition batch cannot be deleted while referenced by one or more products.");
        }

        _dbContext.AcquisitionBatches.Remove(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AcquisitionBatchDeleteResult.Success();
    }

    private async Task<List<Product>> GetProductsForBatchesAsync(List<Guid> batchIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Where(p => p.AcquisitionBatchId != null && batchIds.Contains(p.AcquisitionBatchId.Value))
            .ToListAsync(cancellationToken);
    }

    private static AcquisitionBatchDto BuildDto(AcquisitionBatch batch, IEnumerable<Product> productsInBatch)
    {
        var products = productsInBatch.ToList();
        var partCount = products.Count;

        var revenueSoFar = products
            .Where(p => p.Status == ProductStatus.Sold)
            .Sum(p => p.SoldPrice ?? 0);

        return new AcquisitionBatchDto
        {
            Id = batch.Id,
            Source = batch.Source,
            TotalCost = batch.TotalCost,
            PurchaseDate = batch.PurchaseDate,
            Notes = batch.Notes,
            PartCount = partCount,
            AvailableCount = products.Count(p => p.Status == ProductStatus.Available),
            SoldCount = products.Count(p => p.Status == ProductStatus.Sold),
            HiddenCount = products.Count(p => p.Status == ProductStatus.Hidden),
            EstimatedCostPerPart = partCount > 0 ? batch.TotalCost / partCount : null,
            RevenueSoFar = revenueSoFar,
            ProfitSoFar = revenueSoFar - batch.TotalCost
        };
    }
}
