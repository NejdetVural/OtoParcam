namespace OtoParcam.Application.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ConfirmEmailResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
}
