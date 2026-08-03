using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.VehicleBrands;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/vehicle-brands")]
public class VehicleBrandsController : ControllerBase
{
    private readonly IVehicleBrandService _vehicleBrandService;

    public VehicleBrandsController(IVehicleBrandService vehicleBrandService)
    {
        _vehicleBrandService = vehicleBrandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicleBrands(CancellationToken cancellationToken)
    {
        var vehicleBrands = await _vehicleBrandService.GetVehicleBrandsAsync(cancellationToken);
        return Ok(vehicleBrands);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateVehicleBrand(CreateVehicleBrandRequest request, CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.CreateVehicleBrandAsync(request, cancellationToken);
        return result.Status switch
        {
            VehicleBrandOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.VehicleBrand),
            VehicleBrandOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> UpdateVehicleBrand(Guid id, UpdateVehicleBrandRequest request, CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.UpdateVehicleBrandAsync(id, request, cancellationToken);
        return result.Status switch
        {
            VehicleBrandOperationStatus.Success => NoContent(),
            VehicleBrandOperationStatus.NotFound => NotFound(),
            VehicleBrandOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteVehicleBrand(Guid id, CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.DeleteVehicleBrandAsync(id, cancellationToken);
        return result.Status switch
        {
            VehicleBrandOperationStatus.Success => NoContent(),
            VehicleBrandOperationStatus.NotFound => NotFound(),
            VehicleBrandOperationStatus.Conflict => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }
}
