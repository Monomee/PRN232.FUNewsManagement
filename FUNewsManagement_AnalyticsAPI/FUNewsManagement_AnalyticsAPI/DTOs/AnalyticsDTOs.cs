using System;
using System.Collections.Generic;

namespace FUNewsManagement_AnalyticsAPI.DTOs;

public class DashboardAnalyticsDto
{
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<CategoryStatDto> CategoryStats { get; set; } = new();
    public List<AuthorStatDto> AuthorStats { get; set; } = new();
    public List<StatusStatDto> StatusStats { get; set; } = new();
}

public class CategoryStatDto
{
    public short CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}

public class AuthorStatDto
{
    public short AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public int ArticleCount { get; set; }
}

public class StatusStatDto
{
    public string Status { get; set; } = null!;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TrendingArticleDto
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public int ViewCount { get; set; }
    public string? ImageUrl { get; set; }
}

public class RecommendedArticleDto
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int SharedTagCount { get; set; }
    public bool SameCategory { get; set; }
}
