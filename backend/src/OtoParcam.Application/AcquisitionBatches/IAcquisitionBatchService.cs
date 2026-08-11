namespace OtoParcam.Application.AcquisitionBatches;

public interface IAcquisitionBatchService
{
    Task<IReadOnlyList<AcquisitionBatchDto>> GetBatchesAsync(CancellationToken cancellationToken = default);
    Task<AcquisitionBatchDto?> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AcquisitionBatchResult> CreateBatchAsync(CreateAcquisitionBatchRequest request, CancellationToken cancellationToken = default);
    Task<AcquisitionBatchResult> UpdateBatchAsync(Guid id, UpdateAcquisitionBatchRequest request, CancellationToken cancellationToken = default);
    Task<AcquisitionBatchDeleteResult> DeleteBatchAsync(Guid id, CancellationToken cancellationToken = default);
}
