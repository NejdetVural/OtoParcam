using System.ComponentModel.DataAnnotations;

namespace OtoParcam.Application.VehicleBrands;

public class VehicleBrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateVehicleBrandRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateVehicleBrandRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public enum VehicleBrandOperationStatus
{
    Success,
    NotFound,
    Conflict
}

public class VehicleBrandResult
{
    public VehicleBrandOperationStatus Status { get; init; }
    public VehicleBrandDto? VehicleBrand { get; init; }
    public string? Error { get; init; }

    public static VehicleBrandResult Success(VehicleBrandDto vehicleBrand) => new() { Status = VehicleBrandOperationStatus.Success, VehicleBrand = vehicleBrand };
    public static VehicleBrandResult NotFound() => new() { Status = VehicleBrandOperationStatus.NotFound };
    public static VehicleBrandResult Conflict(string error) => new() { Status = VehicleBrandOperationStatus.Conflict, Error = error };
}

public class VehicleBrandDeleteResult
{
    public VehicleBrandOperationStatus Status { get; init; }
    public string? Error { get; init; }

    public static VehicleBrandDeleteResult Success() => new() { Status = VehicleBrandOperationStatus.Success };
    public static VehicleBrandDeleteResult NotFound() => new() { Status = VehicleBrandOperationStatus.NotFound };
    public static VehicleBrandDeleteResult Conflict(string error) => new() { Status = VehicleBrandOperationStatus.Conflict, Error = error };
}
