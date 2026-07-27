using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Auth;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(new { token = result.Token, expiresAtUtc = result.ExpiresAtUtc });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok();
    }
}
