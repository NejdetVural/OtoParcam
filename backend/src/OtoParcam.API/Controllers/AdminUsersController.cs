using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Users;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = Roles.Administrator)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] AdminUserListQuery query, CancellationToken cancellationToken)
    {
        var users = await _adminUserService.GetUsersAsync(query, cancellationToken);
        return Ok(users);
    }

    [HttpPatch("{id:guid}/promote")]
    public async Task<IActionResult> Promote(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.PromoteToAdministratorAsync(id, cancellationToken);
        return result.Status switch
        {
            AdminUserOperationStatus.Success => Ok(result.User),
            AdminUserOperationStatus.NotFound => NotFound(),
            AdminUserOperationStatus.AlreadyAdministrator => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    [HttpPatch("{id:guid}/demote")]
    public async Task<IActionResult> Demote(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.DemoteToCustomerAsync(id, GetUserId(), cancellationToken);
        return result.Status switch
        {
            AdminUserOperationStatus.Success => Ok(result.User),
            AdminUserOperationStatus.NotFound => NotFound(),
            AdminUserOperationStatus.NotAdministrator => Conflict(new { error = result.Error }),
            AdminUserOperationStatus.CannotDemoteSelf => Conflict(new { error = result.Error }),
            AdminUserOperationStatus.LastAdministrator => Conflict(new { error = result.Error }),
            _ => BadRequest()
        };
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
