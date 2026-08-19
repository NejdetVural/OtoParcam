using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OtoParcam.Application.Auth;
using OtoParcam.Application.Common;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Entities;

namespace OtoParcam.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IEmailSender emailSender,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
    }

    private string FrontendBaseUrl => (_configuration["App:FrontendBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.PrivacyPolicyAccepted)
        {
            return RegisterResult.Failure(new[] { "You must accept the privacy policy to register." });
        }

        var phoneTaken = await _userManager.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (phoneTaken)
        {
            return RegisterResult.Failure(new[] { "Phone number is already taken." });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PrivacyPolicyAcceptedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return RegisterResult.Failure(createResult.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        await SendConfirmationEmailAsync(user, cancellationToken);

        return RegisterResult.Success();
    }

    public async Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.EmailConfirmed)
        {
            // Don't reveal whether the account exists or is already confirmed.
            return;
        }

        await SendConfirmationEmailAsync(user, cancellationToken);
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmLink = $"{FrontendBaseUrl}/onay?userId={user.Id}&token={encodedToken}";

        _logger.LogInformation("Email confirmation link for {Email}: {Link}", user.Email, confirmLink);

        await _emailSender.SendEmailAsync(
            user.Email!,
            "OtoParcam - E-posta Adresinizi Onaylayın",
            $"""
            <p>Merhaba {user.FirstName},</p>
            <p>OtoParcam hesabınızı oluşturduğunuz için teşekkürler. Hesabınızla giriş yapabilmek için
            önce e-posta adresinizi onaylamanız gerekiyor.</p>
            <p><a href="{confirmLink}">E-posta adresimi onayla</a></p>
            <p>Bağlantı çalışmıyorsa şu adresi tarayıcınıza yapıştırabilirsiniz:<br>{confirmLink}</p>
            <p>Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>
            """,
            cancellationToken);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = request.EmailOrPhone.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrPhone)
            : await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.EmailOrPhone, cancellationToken);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return LoginResult.Failure("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            return LoginResult.Failure("Email address is not confirmed.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (jwt, expiresAtUtc) = GenerateJwt(user, roles);

        return LoginResult.Success(jwt, expiresAtUtc);
    }

    public async Task<ConfirmEmailResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ConfirmEmailResult.Failure(new[] { "User not found." });
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return ConfirmEmailResult.Failure(new[] { "Invalid confirmation token." });
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        return result.Succeeded
            ? ConfirmEmailResult.Success()
            : ConfirmEmailResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Don't reveal whether the account exists.
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = $"{FrontendBaseUrl}/sifre-sifirla?userId={user.Id}&token={encodedToken}";

        _logger.LogInformation("Password reset link for {Email}: {Link}", user.Email, resetLink);

        await _emailSender.SendEmailAsync(
            user.Email!,
            "OtoParcam - Şifre Sıfırlama",
            $"""
            <p>Merhaba {user.FirstName},</p>
            <p>OtoParcam hesabınız için bir şifre sıfırlama talebi aldık. Yeni bir şifre belirlemek için
            aşağıdaki bağlantıya tıklayın.</p>
            <p><a href="{resetLink}">Şifremi sıfırla</a></p>
            <p>Bağlantı çalışmıyorsa şu adresi tarayıcınıza yapıştırabilirsiniz:<br>{resetLink}</p>
            <p>Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz, şifreniz değişmeyecektir.</p>
            """,
            cancellationToken);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return ResetPasswordResult.Failure(new[] { "Invalid password reset request." });
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return ResetPasswordResult.Failure(new[] { "Invalid password reset request." });
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        return result.Succeeded
            ? ResetPasswordResult.Success()
            : ResetPasswordResult.Failure(result.Errors.Select(e => e.Description));
    }

    private (string Token, DateTime ExpiresAtUtc) GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiryMinutes"]!));

        var jwt = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
    }
}
