using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using FUNewsManagement_AnalyticsAPI.DataAccess;
using FUNewsManagement_AnalyticsAPI.DTOs;

namespace FUNewsManagement_AnalyticsAPI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AnalyticsDbContext _context;

    public AnalyticsService(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAsync(
        DateTime? startDate, DateTime? endDate, short? categoryId, short? authorId)
    {
        var query = _context.NewsArticles.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(a => a.CreatedDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.CreatedDate <= end);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (authorId.HasValue && authorId.Value > 0)
        {
            query = query.Where(a => a.CreatedById == authorId.Value);
        }

        var articles = await query
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .ToListAsync();

        int total = articles.Count;
        int active = articles.Count(a => a.NewsStatus == true);
        int inactive = total - active;

        // Category stats
        var categoryStats = articles
            .GroupBy(a => new { Id = a.CategoryId ?? 0, Name = a.Category?.CategoryName ?? "Uncategorized" })
            .Select(g => new CategoryStatDto
            {
                CategoryId = (short)g.Key.Id,
                CategoryName = g.Key.Name,
                TotalCount = g.Count(),
                ActiveCount = g.Count(a => a.NewsStatus == true),
                InactiveCount = g.Count(a => a.NewsStatus != true)
            })
            .OrderByDescending(c => c.TotalCount)
            .ToList();

        // Author stats
        var authorStats = articles
            .GroupBy(a => new { Id = a.CreatedById ?? 0, Name = a.CreatedBy?.AccountName ?? "Unknown Author" })
            .Select(g => new AuthorStatDto
            {
                AuthorId = (short)g.Key.Id,
                AuthorName = g.Key.Name,
                ArticleCount = g.Count()
            })
            .OrderByDescending(a => a.ArticleCount)
            .ToList();

        // Status stats
        var statusStats = new List<StatusStatDto>
        {
            new StatusStatDto
            {
                Status = "Active (Published)",
                Count = active,
                Percentage = total > 0 ? Math.Round((double)active / total * 100, 1) : 0
            },
            new StatusStatDto
            {
                Status = "Inactive (Draft)",
                Count = inactive,
                Percentage = total > 0 ? Math.Round((double)inactive / total * 100, 1) : 0
            }
        };

        return new DashboardAnalyticsDto
        {
            TotalArticles = total,
            ActiveArticles = active,
            InactiveArticles = inactive,
            StartDate = startDate,
            EndDate = endDate,
            CategoryStats = categoryStats,
            AuthorStats = authorStats,
            StatusStats = statusStats
        };
    }

    public async Task<List<TrendingArticleDto>> GetTrendingAsync(int top = 5)
    {
        return await _context.NewsArticles.AsNoTracking()
            .Where(a => a.NewsStatus == true)
            .OrderByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.CreatedDate)
            .Take(top)
            .Select(a => new TrendingArticleDto
            {
                NewsArticleId = a.NewsArticleId,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                CreatedDate = a.CreatedDate,
                CategoryName = a.Category != null ? a.Category.CategoryName : null,
                AuthorName = a.CreatedBy != null ? a.CreatedBy.AccountName : null,
                ViewCount = a.ViewCount,
                ImageUrl = a.ImageUrl
            })
            .ToListAsync();
    }

    public async Task<List<RecommendedArticleDto>> GetRecommendationsAsync(string articleId, int top = 3)
    {
        var target = await _context.NewsArticles.AsNoTracking()
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == articleId);

        if (target == null) return new List<RecommendedArticleDto>();

        var targetTagIds = target.Tags.Select(t => t.TagId).ToHashSet();
        var targetCategoryId = target.CategoryId;

        var candidates = await _context.NewsArticles.AsNoTracking()
            .Where(a => a.NewsArticleId != articleId && a.NewsStatus == true)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .ToListAsync();

        var ranked = candidates
            .Select(c =>
            {
                int sharedTags = c.Tags.Count(t => targetTagIds.Contains(t.TagId));
                bool sameCategory = targetCategoryId.HasValue && c.CategoryId == targetCategoryId.Value;
                int score = (sameCategory ? 3 : 0) + (sharedTags * 2);

                return new
                {
                    Article = c,
                    SharedTags = sharedTags,
                    SameCategory = sameCategory,
                    Score = score
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Article.CreatedDate)
            .Take(top)
            .Select(x => new RecommendedArticleDto
            {
                NewsArticleId = x.Article.NewsArticleId,
                NewsTitle = x.Article.NewsTitle,
                Headline = x.Article.Headline,
                CreatedDate = x.Article.CreatedDate,
                CategoryName = x.Article.Category?.CategoryName,
                ImageUrl = x.Article.ImageUrl,
                SharedTagCount = x.SharedTags,
                SameCategory = x.SameCategory
            })
            .ToList();

        return ranked;
    }

    public async Task ExportExcelToStreamAsync(
        DateTime? startDate, DateTime? endDate, short? categoryId, short? authorId, Stream outputStream)
    {
        var dashboard = await GetDashboardAsync(startDate, endDate, categoryId, authorId);

        var query = _context.NewsArticles.AsNoTracking().AsQueryable();
        if (startDate.HasValue) query = query.Where(a => a.CreatedDate >= startDate.Value.Date);
        if (endDate.HasValue) query = query.Where(a => a.CreatedDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        if (categoryId.HasValue && categoryId.Value > 0) query = query.Where(a => a.CategoryId == categoryId.Value);
        if (authorId.HasValue && authorId.Value > 0) query = query.Where(a => a.CreatedById == authorId.Value);

        var articles = await query
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        using var workbook = new XLWorkbook();

        // Sheet 1: Executive KPI & Summary
        var wsOverview = workbook.Worksheets.Add("KPI & Statistics");
        wsOverview.Cell(1, 1).Value = "FUNews Management System - Analytics & Report";
        wsOverview.Cell(1, 1).Style.Font.Bold = true;
        wsOverview.Cell(1, 1).Style.Font.FontSize = 16;
        wsOverview.Range(1, 1, 1, 5).Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(13, 110, 253);
        wsOverview.Cell(1, 1).Style.Font.FontColor = XLColor.White;

        wsOverview.Cell(3, 1).Value = "Generated At:";
        wsOverview.Cell(3, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
        wsOverview.Cell(4, 1).Value = "Total Articles:";
        wsOverview.Cell(4, 2).Value = dashboard.TotalArticles;
        wsOverview.Cell(5, 1).Value = "Active Articles:";
        wsOverview.Cell(5, 2).Value = dashboard.ActiveArticles;
        wsOverview.Cell(6, 1).Value = "Inactive Articles:";
        wsOverview.Cell(6, 2).Value = dashboard.InactiveArticles;

        // Categories Table
        wsOverview.Cell(8, 1).Value = "Category Breakdown";
        wsOverview.Cell(8, 1).Style.Font.Bold = true;
        wsOverview.Cell(9, 1).Value = "Category ID";
        wsOverview.Cell(9, 2).Value = "Category Name";
        wsOverview.Cell(9, 3).Value = "Total Articles";
        wsOverview.Cell(9, 4).Value = "Active";
        wsOverview.Cell(9, 5).Value = "Inactive";
        wsOverview.Range(9, 1, 9, 5).Style.Font.Bold = true;
        wsOverview.Range(9, 1, 9, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 10;
        foreach (var c in dashboard.CategoryStats)
        {
            wsOverview.Cell(row, 1).Value = c.CategoryId;
            wsOverview.Cell(row, 2).Value = c.CategoryName;
            wsOverview.Cell(row, 3).Value = c.TotalCount;
            wsOverview.Cell(row, 4).Value = c.ActiveCount;
            wsOverview.Cell(row, 5).Value = c.InactiveCount;
            row++;
        }

        // Sheet 2: Detailed Articles
        var wsArticles = workbook.Worksheets.Add("Articles Detail");
        wsArticles.Cell(1, 1).Value = "Article ID";
        wsArticles.Cell(1, 2).Value = "Title";
        wsArticles.Cell(1, 3).Value = "Headline";
        wsArticles.Cell(1, 4).Value = "Category";
        wsArticles.Cell(1, 5).Value = "Author";
        wsArticles.Cell(1, 6).Value = "Status";
        wsArticles.Cell(1, 7).Value = "Created Date";
        wsArticles.Cell(1, 8).Value = "Views";
        wsArticles.Cell(1, 9).Value = "Tags";
        wsArticles.Range(1, 1, 1, 9).Style.Font.Bold = true;
        wsArticles.Range(1, 1, 1, 9).Style.Fill.BackgroundColor = XLColor.FromArgb(25, 135, 84);
        wsArticles.Range(1, 1, 1, 9).Style.Font.FontColor = XLColor.White;

        int artRow = 2;
        foreach (var a in articles)
        {
            wsArticles.Cell(artRow, 1).Value = a.NewsArticleId;
            wsArticles.Cell(artRow, 2).Value = a.NewsTitle ?? "";
            wsArticles.Cell(artRow, 3).Value = a.Headline;
            wsArticles.Cell(artRow, 4).Value = a.Category?.CategoryName ?? "N/A";
            wsArticles.Cell(artRow, 5).Value = a.CreatedBy?.AccountName ?? "N/A";
            wsArticles.Cell(artRow, 6).Value = (a.NewsStatus == true) ? "Active" : "Inactive";
            wsArticles.Cell(artRow, 7).Value = a.CreatedDate.HasValue ? a.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm") : "";
            wsArticles.Cell(artRow, 8).Value = a.ViewCount;
            wsArticles.Cell(artRow, 9).Value = string.Join(", ", a.Tags.Select(t => t.TagName));
            artRow++;
        }

        wsOverview.Columns().AdjustToContents();
        wsArticles.Columns().AdjustToContents();

        // Stream directly to response body without intermediate RAM buffers!
        workbook.SaveAs(outputStream);
    }
}
