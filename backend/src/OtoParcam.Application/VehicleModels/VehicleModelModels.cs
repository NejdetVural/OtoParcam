using System.ComponentModel.DataAnnotations;

namespace OtoParcam.Application.VehicleModels;

public class VehicleModelDto
{
    public Guid Id { get; set; }
    public Guid VehicleBrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public short StartYear { get; set; }
    public short EndYear { get; set; }
    public string? Variant { get; set; }
}

public class CreateVehicleModelRequest
{
    [Required]
    public Guid VehicleBrandId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public short StartYear { get; set; }

    [Required]
    public short EndYear { get; set; }

    [MaxLength(100)]
    public string? Variant { get; set; }
}

public class UpdateVehicleModelRequest
{
    [Required]
    public Guid VehicleBrandId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public short StartYear { get; set; }

    [Required]
    public short EndYear { get; set; }

    [MaxLength(100)]
    public string? Variant { get; set; }
}

public enum VehicleModelOperationStatus
{
    Success,
    NotFound,
    InvalidVehicleBrand,
    InvalidYearRange,
    Conflict
}

public class VehicleModelResult
{
    public VehicleModelOperationStatus Status { get; init; }
    public VehicleModelDto? VehicleModel { get; init; }
    public string? Error { get; init; }

    public static VehicleModelResult Success(VehicleModelDto vehicleModel) => new() { Status = VehicleModelOperationStatus.Success, VehicleModel = vehicleModel };
    public static VehicleModelResult NotFound() => new() { Status = VehicleModelOperationStatus.NotFound };
    public static VehicleModelResult InvalidVehicleBrand(string error) => new() { Status = VehicleModelOperationStatus.InvalidVehicleBrand, Error = error };
    public static VehicleModelResult InvalidYearRange(string error) => new() { Status = VehicleModelOperationStatus.InvalidYearRange, Error = error };
}

public class VehicleModelDeleteResult
{
    public VehicleModelOperationStatus Status { get; init; }
    public string? Error { get; init; }

    public static VehicleModelDeleteResult Success() => new() { Status = VehicleModelOperationStatus.Success };
    public static VehicleModelDeleteResult NotFound() => new() { Status = VehicleModelOperationStatus.NotFound };
    public static VehicleModelDeleteResult Conflict(string error) => new() { Status = VehicleModelOperationStatus.Conflict, Error = error };
}
