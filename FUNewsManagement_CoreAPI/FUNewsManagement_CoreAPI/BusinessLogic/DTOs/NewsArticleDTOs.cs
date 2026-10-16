using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class NewsArticleDto
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? NewsStatus { get; set; }
    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public short? UpdatedById { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ImageUrl { get; set; }
    public int ViewCount { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}

public class CreateNewsArticleDto
{
    [StringLength(20, ErrorMessage = "NewsArticleID cannot exceed 20 characters")]
    public string? NewsArticleId { get; set; }

    [Required(ErrorMessage = "News Title is required")]
    [StringLength(400, ErrorMessage = "News Title cannot exceed 400 characters")]
    public string NewsTitle { get; set; } = null!;

    [Required(ErrorMessage = "Headline is required")]
    [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
    public string Headline { get; set; } = null!;

    [Required(ErrorMessage = "News Content is required")]
    [StringLength(4000, ErrorMessage = "News Content cannot exceed 4000 characters")]
    public string NewsContent { get; set; } = null!;

    [StringLength(400, ErrorMessage = "News Source cannot exceed 400 characters")]
    public string? NewsSource { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public short CategoryId { get; set; }

    public bool NewsStatus { get; set; } = true;

    [StringLength(500, ErrorMessage = "ImageUrl cannot exceed 500 characters")]
    public string? ImageUrl { get; set; }

    public List<int>? TagIds { get; set; }
}

public class UpdateNewsArticleDto
{
    [Required(ErrorMessage = "News Title is required")]
    [StringLength(400, ErrorMessage = "News Title cannot exceed 400 characters")]
    public string NewsTitle { get; set; } = null!;

    [Required(ErrorMessage = "Headline is required")]
    [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
    public string Headline { get; set; } = null!;

    [Required(ErrorMessage = "News Content is required")]
    [StringLength(4000, ErrorMessage = "News Content cannot exceed 4000 characters")]
    public string NewsContent { get; set; } = null!;

    [StringLength(400, ErrorMessage = "News Source cannot exceed 400 characters")]
    public string? NewsSource { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public short CategoryId { get; set; }

    public bool NewsStatus { get; set; }

    [StringLength(500, ErrorMessage = "ImageUrl cannot exceed 500 characters")]
    public string? ImageUrl { get; set; }

    public List<int>? TagIds { get; set; }
}
