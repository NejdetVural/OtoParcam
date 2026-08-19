using System.ComponentModel.DataAnnotations;

namespace OtoParcam.Application.AcquisitionBatches;

public class AcquisitionBatchDto
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int PartCount { get; set; }
    public int AvailableCount { get; set; }
    public int SoldCount { get; set; }
    public int HiddenCount { get; set; }
    public decimal? EstimatedCostPerPart { get; set; }
    public decimal RevenueSoFar { get; set; }
    public decimal ProfitSoFar { get; set; }
}

public class CreateAcquisitionBatchRequest
{
    [Required, MaxLength(500)]
    public string Source { get; set; } = string.Empty;

    [Required]
    public decimal TotalCost { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateAcquisitionBatchRequest
{
    [Required, MaxLength(500)]
    public string Source { get; set; } = string.Empty;

    [Required]
    public decimal TotalCost { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public enum AcquisitionBatchOperationStatus
{
    Success,
    NotFound,
    Conflict,
    InvalidCost
}

public class AcquisitionBatchResult
{
    public AcquisitionBatchOperationStatus Status { get; init; }
    public AcquisitionBatchDto? Batch { get; init; }
    public string? Error { get; init; }

    public static AcquisitionBatchResult Success(AcquisitionBatchDto batch) => new() { Status = AcquisitionBatchOperationStatus.Success, Batch = batch };
    public static AcquisitionBatchResult NotFound() => new() { Status = AcquisitionBatchOperationStatus.NotFound };
    public static AcquisitionBatchResult InvalidCost(string error) => new() { Status = AcquisitionBatchOperationStatus.InvalidCost, Error = error };
    public static AcquisitionBatchResult Conflict(string error) => new() { Status = AcquisitionBatchOperationStatus.Conflict, Error = error };
}

public class AcquisitionBatchDeleteResult
{
    public AcquisitionBatchOperationStatus Status { get; init; }
    public string? Error { get; init; }

    public static AcquisitionBatchDeleteResult Success() => new() { Status = AcquisitionBatchOperationStatus.Success };
    public static AcquisitionBatchDeleteResult NotFound() => new() { Status = AcquisitionBatchOperationStatus.NotFound };
    public static AcquisitionBatchDeleteResult Conflict(string error) => new() { Status = AcquisitionBatchOperationStatus.Conflict, Error = error };
}
