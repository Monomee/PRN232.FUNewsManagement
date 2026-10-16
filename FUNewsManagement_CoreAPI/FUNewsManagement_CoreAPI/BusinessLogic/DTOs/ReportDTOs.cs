using System;
using System.Collections.Generic;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class ReportStatisticsDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public List<CategoryGroupReportDto> CategoryGroups { get; set; } = new List<CategoryGroupReportDto>();
    public List<AuthorGroupReportDto> AuthorGroups { get; set; } = new List<AuthorGroupReportDto>();
    public List<NewsArticleDto> Articles { get; set; } = new List<NewsArticleDto>();
}

public class CategoryGroupReportDto
{
    public short? CategoryId { get; set; }
    public string CategoryName { get; set; } = "Uncategorized";
    public int ArticleCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}

public class AuthorGroupReportDto
{
    public short? AuthorId { get; set; }
    public string AuthorName { get; set; } = "Unknown";
    public int ArticleCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}
