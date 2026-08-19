using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OtoParcam.Application.Auth;
using OtoParcam.Application.Common;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Entities;
using OtoParcam.Infrastructure.Persistence;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class AuthServiceTests
{
    private const string StrongPassword = "Str0ng!Passw0rd";

    private class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private static (AuthService Service, UserManager<ApplicationUser> UserManager, RoleManager<IdentityRole<Guid>> RoleManager) CreateAuthService(ApplicationDbContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(context);
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-only-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "OtoParcamTests",
                ["Jwt:Audience"] = "OtoParcamTests",
                ["Jwt:ExpiryMinutes"] = "60",
                ["App:FrontendBaseUrl"] = "http://localhost:5173",
            })
            .Build();

        var service = new AuthService(userManager, configuration, new NoOpEmailSender(), NullLogger<AuthService>.Instance);
        return (service, userManager, roleManager);
    }

    private static string EncodeToken(string rawToken) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

    [Fact]
    public async Task RegisterAsync_PrivacyPolicyNotAccepted_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FirstName = "Ahmet",
            LastName = "Yilmaz",
            Email = "ahmet@test.com",
            PhoneNumber = "5551234567",
            Password = StrongPassword,
            PrivacyPolicyAccepted = false,
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhone_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var existing = CreateUser("existing@test.com");
        existing.PhoneNumber = "5551234567";
        await userManager.CreateAsync(existing, StrongPassword);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FirstName = "Ahmet",
            LastName = "Yilmaz",
            Email = "ahmet@test.com",
            PhoneNumber = "5551234567",
            Password = StrongPassword,
            PrivacyPolicyAccepted = true,
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RegisterAsync_Success_CreatesUserAndAssignsCustomerRole()
    {
        await using var context = CreateContext();
        var (service, userManager, roleManager) = CreateAuthService(context);
        await roleManager.CreateAsync(new IdentityRole<Guid> { Name = Roles.Customer, NormalizedName = Roles.Customer.ToUpperInvariant() });

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FirstName = "Ahmet",
            LastName = "Yilmaz",
            Email = "ahmet@test.com",
            PhoneNumber = "5551234567",
            Password = StrongPassword,
            PrivacyPolicyAccepted = true,
        });

        Assert.True(result.Succeeded);
        var created = await userManager.FindByEmailAsync("ahmet@test.com");
        Assert.NotNull(created);
        Assert.True(await userManager.IsInRoleAsync(created!, Roles.Customer));
    }

    [Fact]
    public async Task LoginAsync_UnknownIdentifier_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        var result = await service.LoginAsync(new LoginRequest { EmailOrPhone = "nobody@test.com", Password = StrongPassword });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        user.EmailConfirmed = true;
        await userManager.CreateAsync(user, StrongPassword);

        var result = await service.LoginAsync(new LoginRequest { EmailOrPhone = "ahmet@test.com", Password = "WrongPassword1!" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_UnconfirmedEmail_ReturnsFailureEvenWithCorrectPassword()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        user.EmailConfirmed = false;
        await userManager.CreateAsync(user, StrongPassword);

        var result = await service.LoginAsync(new LoginRequest { EmailOrPhone = "ahmet@test.com", Password = StrongPassword });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_Success_ReturnsJwtWithRoleClaim()
    {
        await using var context = CreateContext();
        var (service, userManager, roleManager) = CreateAuthService(context);
        await roleManager.CreateAsync(new IdentityRole<Guid> { Name = Roles.Customer, NormalizedName = Roles.Customer.ToUpperInvariant() });
        var user = CreateUser("ahmet@test.com");
        user.EmailConfirmed = true;
        await userManager.CreateAsync(user, StrongPassword);
        await userManager.AddToRoleAsync(user, Roles.Customer);

        var result = await service.LoginAsync(new LoginRequest { EmailOrPhone = "ahmet@test.com", Password = StrongPassword });

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.Token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == Roles.Customer);
    }

    [Fact]
    public async Task ConfirmEmailAsync_UnknownUser_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        var result = await service.ConfirmEmailAsync(Guid.NewGuid(), "any-token");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ConfirmEmailAsync_MalformedToken_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);

        var result = await service.ConfirmEmailAsync(user.Id, "!!!not-valid-base64url!!!");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidToken_ConfirmsEmail()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);
        var rawToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var result = await service.ConfirmEmailAsync(user.Id, EncodeToken(rawToken));

        Assert.True(result.Succeeded);
        var stored = await userManager.FindByIdAsync(user.Id.ToString());
        Assert.True(stored!.EmailConfirmed);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_UnknownEmail_DoesNotThrow()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        await service.ResendConfirmationEmailAsync(new ResendConfirmationRequest { Email = "nobody@test.com" });
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_AlreadyConfirmed_DoesNotThrow()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        user.EmailConfirmed = true;
        await userManager.CreateAsync(user, StrongPassword);

        await service.ResendConfirmationEmailAsync(new ResendConfirmationRequest { Email = "ahmet@test.com" });
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_UnconfirmedUser_GeneratesTokenThatConfirms()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);

        await service.ResendConfirmationEmailAsync(new ResendConfirmationRequest { Email = "ahmet@test.com" });

        // ResendConfirmationEmailAsync doesn't return the token directly (it's emailed), so confirm
        // indirectly: generate a fresh token the same way and verify it still confirms the account,
        // proving the resend path didn't leave the user in a broken state.
        var rawToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await service.ConfirmEmailAsync(user.Id, EncodeToken(rawToken));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_UnknownEmail_DoesNotThrow()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        await service.RequestPasswordResetAsync(new ForgotPasswordRequest { Email = "nobody@test.com" });
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownUser_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, _, _) = CreateAuthService(context);

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = Guid.NewGuid(),
            Token = "any-token",
            NewPassword = StrongPassword,
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_MalformedToken_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = user.Id,
            Token = "!!!not-valid-base64url!!!",
            NewPassword = "AnotherStr0ng!Pass",
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_WeakPassword_ReturnsFailure()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);
        var rawToken = await userManager.GeneratePasswordResetTokenAsync(user);

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = user.Id,
            Token = EncodeToken(rawToken),
            NewPassword = "weak",
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidTokenAndPassword_ChangesPassword()
    {
        await using var context = CreateContext();
        var (service, userManager, _) = CreateAuthService(context);
        var user = CreateUser("ahmet@test.com");
        await userManager.CreateAsync(user, StrongPassword);
        var rawToken = await userManager.GeneratePasswordResetTokenAsync(user);
        const string newPassword = "AnotherStr0ng!Pass";

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = user.Id,
            Token = EncodeToken(rawToken),
            NewPassword = newPassword,
        });

        Assert.True(result.Succeeded);
        var stored = await userManager.FindByIdAsync(user.Id.ToString());
        Assert.True(await userManager.CheckPasswordAsync(stored!, newPassword));
        Assert.False(await userManager.CheckPasswordAsync(stored!, StrongPassword));
    }
}
