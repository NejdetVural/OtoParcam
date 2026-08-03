using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Users;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = Roles.Customer)]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetOwnProfile(CancellationToken cancellationToken)
    {
        var profile = await _userProfileService.GetProfileAsync(GetUserId(), cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateOwnProfile(UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileService.UpdateProfileAsync(GetUserId(), request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
