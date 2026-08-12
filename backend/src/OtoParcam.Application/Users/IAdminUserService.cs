namespace OtoParcam.Application.Users;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(AdminUserListQuery query, CancellationToken cancellationToken = default);
    Task<AdminUserRoleResult> PromoteToAdministratorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserRoleResult> DemoteToCustomerAsync(Guid userId, Guid actingAdminId, CancellationToken cancellationToken = default);
}
