using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Users;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;

namespace OtoParcam.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(AdminUserListQuery query, CancellationToken cancellationToken = default)
    {
        var administratorRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Administrator)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IQueryable<ApplicationUser> users = _dbContext.Users;

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword;
            users = users.Where(u =>
                u.FirstName.Contains(keyword) ||
                u.LastName.Contains(keyword) ||
                (u.Email != null && u.Email.Contains(keyword)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
        }

        return await users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                EmailConfirmed = u.EmailConfirmed,
                IsAdministrator = _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == administratorRoleId),
                CreatedAt = u.CreatedAt,
                FavoriteCount = u.Favorites.Count,
                PurchaseRequestCount = u.PurchaseRequests.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUserRoleResult> PromoteToAdministratorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return AdminUserRoleResult.NotFound();
        }

        if (await _userManager.IsInRoleAsync(user, Roles.Administrator))
        {
            return AdminUserRoleResult.AlreadyAdministrator();
        }

        await _userManager.AddToRoleAsync(user, Roles.Administrator);
        return AdminUserRoleResult.Success(await ToDtoAsync(user, cancellationToken));
    }

    public async Task<AdminUserRoleResult> DemoteToCustomerAsync(Guid userId, Guid actingAdminId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return AdminUserRoleResult.NotFound();
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Administrator))
        {
            return AdminUserRoleResult.NotAdministrator();
        }

        if (user.Id == actingAdminId)
        {
            return AdminUserRoleResult.CannotDemoteSelf();
        }

        var administratorRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Administrator)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var administratorCount = await _dbContext.UserRoles
            .CountAsync(ur => ur.RoleId == administratorRoleId, cancellationToken);
        if (administratorCount <= 1)
        {
            return AdminUserRoleResult.LastAdministrator();
        }

        await _userManager.RemoveFromRoleAsync(user, Roles.Administrator);
        return AdminUserRoleResult.Success(await ToDtoAsync(user, cancellationToken));
    }

    private async Task<AdminUserDto> ToDtoAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var favoriteCount = await _dbContext.Favorites.CountAsync(f => f.ApplicationUserId == user.Id, cancellationToken);
        var purchaseRequestCount = await _dbContext.PurchaseRequests.CountAsync(r => r.ApplicationUserId == user.Id, cancellationToken);
        var isAdministrator = await _userManager.IsInRoleAsync(user, Roles.Administrator);

        return new AdminUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            IsAdministrator = isAdministrator,
            CreatedAt = user.CreatedAt,
            FavoriteCount = favoriteCount,
            PurchaseRequestCount = purchaseRequestCount
        };
    }
}
