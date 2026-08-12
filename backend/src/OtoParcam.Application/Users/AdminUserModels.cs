namespace OtoParcam.Application.Users;

public class AdminUserListQuery
{
    public string? Keyword { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsAdministrator { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FavoriteCount { get; set; }
    public int PurchaseRequestCount { get; set; }
}

public enum AdminUserOperationStatus
{
    Success,
    NotFound,
    AlreadyAdministrator,
    NotAdministrator,
    CannotDemoteSelf,
    LastAdministrator
}

public class AdminUserRoleResult
{
    public AdminUserOperationStatus Status { get; init; }
    public AdminUserDto? User { get; init; }
    public string? Error { get; init; }

    public static AdminUserRoleResult Success(AdminUserDto user) => new() { Status = AdminUserOperationStatus.Success, User = user };
    public static AdminUserRoleResult NotFound() => new() { Status = AdminUserOperationStatus.NotFound };
    public static AdminUserRoleResult AlreadyAdministrator() =>
        new() { Status = AdminUserOperationStatus.AlreadyAdministrator, Error = "User is already an administrator." };
    public static AdminUserRoleResult NotAdministrator() =>
        new() { Status = AdminUserOperationStatus.NotAdministrator, Error = "User is not an administrator." };
    public static AdminUserRoleResult CannotDemoteSelf() =>
        new() { Status = AdminUserOperationStatus.CannotDemoteSelf, Error = "You cannot remove your own administrator role." };
    public static AdminUserRoleResult LastAdministrator() =>
        new() { Status = AdminUserOperationStatus.LastAdministrator, Error = "Cannot demote the last remaining administrator." };
}
