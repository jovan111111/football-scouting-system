using System.ComponentModel.DataAnnotations;

namespace ScoutBoard.Api.DTOs;

public class RegisterRequest
{
    [Required, MaxLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$")]
    public string Code { get; set; } = string.Empty;
}

public class ResendOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record CurrentUserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    int? PlayerProfileId);

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    CurrentUserDto User);

public record MessageResponse(string Message);
