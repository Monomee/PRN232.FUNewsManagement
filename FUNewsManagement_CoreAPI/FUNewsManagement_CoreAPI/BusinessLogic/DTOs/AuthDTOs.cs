using System;
using System.ComponentModel.DataAnnotations;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public short AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int? AccountRole { get; set; }
}

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "RefreshToken is required")]
    public string RefreshToken { get; set; } = null!;
}
