using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.AcquisitionBatches;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/admin/acquisition-batches")]
[Authorize(Roles = Roles.Administrator)]
public class AcquisitionBatchesController : ControllerBase
{
    private readonly IAcquisitionBatchService _acquisitionBatchService;

    public AcquisitionBatchesController(IAcquisitionBatchService acquisitionBatchService)
    {
        _acquisitionBatchService = acquisitionBatchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBatches(CancellationToken cancellationToken)
    {
        var batches = await _acquisitionBatchService.GetBatchesAsync(cancellationToken);
        return Ok(batches);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBatchById(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _acquisitionBatchService.GetBatchByIdAsync(id, cancellationToken);
        return batch is null ? NotFound() : Ok(batch);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBatch(CreateAcquisitionBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _acquisitionBatchService.CreateBatchAsync(request, cancellationToken);
        return result.Status switch
        {
            AcquisitionBatchOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.Batch),
            AcquisitionBatchOperationStatus.InvalidCost => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBatch(Guid id, UpdateAcquisitionBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _acquisitionBatchService.UpdateBatchAsync(id, request, cancellationToken);
        return result.Status switch
        {
            AcquisitionBatchOperationStatus.Success => Ok(result.Batch),
            AcquisitionBatchOperationStatus.NotFound => NotFound(),
            AcquisitionBatchOperationStatus.InvalidCost => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> CloseBatch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _acquisitionBatchService.CloseBatchAsync(id, cancellationToken);
        return result.Status switch
        {
            AcquisitionBatchOperationStatus.Success => Ok(result.Batch),
            AcquisitionBatchOperationStatus.NotFound => NotFound(),
            AcquisitionBatchOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/reopen")]
    public async Task<IActionResult> ReopenBatch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _acquisitionBatchService.ReopenBatchAsync(id, cancellationToken);
        return result.Status switch
        {
            AcquisitionBatchOperationStatus.Success => Ok(result.Batch),
            AcquisitionBatchOperationStatus.NotFound => NotFound(),
            AcquisitionBatchOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBatch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _acquisitionBatchService.DeleteBatchAsync(id, cancellationToken);
        return result.Status switch
        {
            AcquisitionBatchOperationStatus.Success => NoContent(),
            AcquisitionBatchOperationStatus.NotFound => NotFound(),
            AcquisitionBatchOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }
}
