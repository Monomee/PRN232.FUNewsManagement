using System.ComponentModel.DataAnnotations;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class TagDto
{
    public int TagId { get; set; }
    public string? TagName { get; set; }
    public string? Note { get; set; }
    public int ArticleCount { get; set; }
}

public class CreateTagDto
{
    [Required(ErrorMessage = "Tag Name is required")]
    [StringLength(50, ErrorMessage = "Tag Name cannot exceed 50 characters")]
    public string TagName { get; set; } = null!;

    [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
    public string? Note { get; set; }
}

public class UpdateTagDto
{
    [Required(ErrorMessage = "Tag Name is required")]
    [StringLength(50, ErrorMessage = "Tag Name cannot exceed 50 characters")]
    public string TagName { get; set; } = null!;

    [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
    public string? Note { get; set; }
}
