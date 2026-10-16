using System.ComponentModel.DataAnnotations;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class SystemAccountDto
{
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public int? AccountRole { get; set; }
    public string RoleName => AccountRole switch
    {
        1 => "Staff",
        2 => "Lecturer",
        _ => "Admin"
    };
    public int CreatedArticlesCount { get; set; }
}

public class CreateAccountDto
{
    [Required(ErrorMessage = "Account Name is required")]
    [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Account Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(70, ErrorMessage = "Account Email cannot exceed 70 characters")]
    public string AccountEmail { get; set; } = null!;

    [Required(ErrorMessage = "Account Role is required")]
    [Range(1, 2, ErrorMessage = "Role must be 1 (Staff) or 2 (Lecturer)")]
    public int AccountRole { get; set; }

    [Required(ErrorMessage = "Account Password is required")]
    [StringLength(70, MinimumLength = 2, ErrorMessage = "Password must be at least 2 characters")]
    public string AccountPassword { get; set; } = null!;
}

public class UpdateAccountDto
{
    [Required(ErrorMessage = "Account Name is required")]
    [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Account Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(70, ErrorMessage = "Account Email cannot exceed 70 characters")]
    public string AccountEmail { get; set; } = null!;

    [Required(ErrorMessage = "Account Role is required")]
    [Range(1, 2, ErrorMessage = "Role must be 1 (Staff) or 2 (Lecturer)")]
    public int AccountRole { get; set; }

    [StringLength(70, MinimumLength = 2, ErrorMessage = "Password must be at least 2 characters")]
    public string? AccountPassword { get; set; }
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(70, MinimumLength = 2, ErrorMessage = "Password must be at least 2 characters")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}
