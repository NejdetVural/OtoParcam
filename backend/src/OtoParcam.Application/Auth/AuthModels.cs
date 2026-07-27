using System.ComponentModel.DataAnnotations;

namespace OtoParcam.Application.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static RegisterResult Success() => new() { Succeeded = true };
    public static RegisterResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToArray() };
}

public class LoginResult
{
    public bool Succeeded { get; init; }
    public string? Token { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static LoginResult Success(string token, DateTime expiresAtUtc) =>
        new() { Succeeded = true, Token = token, ExpiresAtUtc = expiresAtUtc };
    public static LoginResult Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}

public class ConfirmEmailResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ConfirmEmailResult Success() => new() { Succeeded = true };
    public static ConfirmEmailResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToArray() };
}
