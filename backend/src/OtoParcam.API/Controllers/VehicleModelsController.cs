using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.VehicleModels;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/vehicle-models")]
public class VehicleModelsController : ControllerBase
{
    private readonly IVehicleModelService _vehicleModelService;

    public VehicleModelsController(IVehicleModelService vehicleModelService)
    {
        _vehicleModelService = vehicleModelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicleModels(CancellationToken cancellationToken)
    {
        var vehicleModels = await _vehicleModelService.GetVehicleModelsAsync(cancellationToken);
        return Ok(vehicleModels);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateVehicleModel(CreateVehicleModelRequest request, CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.CreateVehicleModelAsync(request, cancellationToken);
        return result.Status switch
        {
            VehicleModelOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.VehicleModel),
            VehicleModelOperationStatus.InvalidVehicleBrand => BadRequest(new { error = result.Error }),
            VehicleModelOperationStatus.InvalidYearRange => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> UpdateVehicleModel(Guid id, UpdateVehicleModelRequest request, CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.UpdateVehicleModelAsync(id, request, cancellationToken);
        return result.Status switch
        {
            VehicleModelOperationStatus.Success => NoContent(),
            VehicleModelOperationStatus.NotFound => NotFound(),
            VehicleModelOperationStatus.InvalidVehicleBrand => BadRequest(new { error = result.Error }),
            VehicleModelOperationStatus.InvalidYearRange => BadRequest(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteVehicleModel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.DeleteVehicleModelAsync(id, cancellationToken);
        return result.Status switch
        {
            VehicleModelOperationStatus.Success => NoContent(),
            VehicleModelOperationStatus.NotFound => NotFound(),
            VehicleModelOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }
}
