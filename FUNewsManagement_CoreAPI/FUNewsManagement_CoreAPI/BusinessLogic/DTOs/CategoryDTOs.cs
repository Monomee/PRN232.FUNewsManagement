using System.ComponentModel.DataAnnotations;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class CategoryDto
{
    public short CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryDesciption { get; set; } = null!;
    public short? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool? IsActive { get; set; }
    public int ArticleCount { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Category Description is required")]
    [StringLength(250, ErrorMessage = "Category Description cannot exceed 250 characters")]
    public string CategoryDesciption { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; } = true;
}

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Category Description is required")]
    [StringLength(250, ErrorMessage = "Category Description cannot exceed 250 characters")]
    public string CategoryDesciption { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }
}
