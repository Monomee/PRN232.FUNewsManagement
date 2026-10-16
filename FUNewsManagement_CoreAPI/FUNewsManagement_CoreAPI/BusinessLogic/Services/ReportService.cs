using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISystemAccountRepository _accountRepository;

    public ReportService(
        INewsArticleRepository articleRepository,
        ICategoryRepository categoryRepository,
        ISystemAccountRepository accountRepository)
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<ReportStatisticsDto> GetReportStatisticsAsync(DateTime? startDate, DateTime? endDate)
    {
        var allArticles = await _articleRepository.GetAllNewsArticlesAsync();
        var allCategories = await _categoryRepository.GetAllCategoriesAsync();
        var allAccounts = await _accountRepository.GetAllAccountsAsync();

        var accountDict = allAccounts.ToDictionary(a => a.AccountId, a => a.AccountName ?? "User");
        var categoryDict = allCategories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

        // Apply Date Filtering
        var query = allArticles.AsEnumerable();
        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(a => a.CreatedDate.HasValue && a.CreatedDate.Value >= start);
        }
        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.CreatedDate.HasValue && a.CreatedDate.Value <= end);
        }

        // Sort descending by CreatedDate
        var filteredArticles = query.OrderByDescending(a => a.CreatedDate).ToList();

        var articleDtos = filteredArticles.Select(a => new NewsArticleDto
        {
            NewsArticleId = a.NewsArticleId,
            NewsTitle = a.NewsTitle,
            Headline = a.Headline,
            CreatedDate = a.CreatedDate,
            NewsContent = a.NewsContent,
            NewsSource = a.NewsSource,
            CategoryId = a.CategoryId,
            CategoryName = a.CategoryId.HasValue && categoryDict.TryGetValue(a.CategoryId.Value, out var cName) ? cName : "Uncategorized",
            NewsStatus = a.NewsStatus,
            CreatedById = a.CreatedById,
            CreatedByName = a.CreatedById.HasValue && accountDict.TryGetValue(a.CreatedById.Value, out var aName) ? aName : "Unknown",
            UpdatedById = a.UpdatedById,
            UpdatedByName = a.UpdatedById.HasValue && accountDict.TryGetValue(a.UpdatedById.Value, out var uName) ? uName : null,
            ModifiedDate = a.ModifiedDate,
            Tags = a.Tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                TagName = t.TagName,
                Note = t.Note
            }).ToList()
        }).ToList();

        // Group by Category
        var categoryGroups = articleDtos
            .GroupBy(a => a.CategoryId)
            .Select(g => new CategoryGroupReportDto
            {
                CategoryId = g.Key,
                CategoryName = g.FirstOrDefault()?.CategoryName ?? "Uncategorized",
                ArticleCount = g.Count(),
                ActiveCount = g.Count(a => a.NewsStatus == true),
                InactiveCount = g.Count(a => a.NewsStatus != true)
            })
            .OrderByDescending(cg => cg.ArticleCount)
            .ToList();

        // Group by Author
        var authorGroups = articleDtos
            .GroupBy(a => a.CreatedById)
            .Select(g => new AuthorGroupReportDto
            {
                AuthorId = g.Key,
                AuthorName = g.FirstOrDefault()?.CreatedByName ?? "Unknown",
                ArticleCount = g.Count(),
                ActiveCount = g.Count(a => a.NewsStatus == true),
                InactiveCount = g.Count(a => a.NewsStatus != true)
            })
            .OrderByDescending(ag => ag.ArticleCount)
            .ToList();

        return new ReportStatisticsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalArticles = articleDtos.Count,
            ActiveArticles = articleDtos.Count(a => a.NewsStatus == true),
            InactiveArticles = articleDtos.Count(a => a.NewsStatus != true),
            CategoryGroups = categoryGroups,
            AuthorGroups = authorGroups,
            Articles = articleDtos
        };
    }

    public async Task<byte[]> ExportReportToExcelAsync(DateTime? startDate, DateTime? endDate)
    {
        var report = await GetReportStatisticsAsync(startDate, endDate);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("News Statistics Report");

        // Title Header
        ws.Cell("A1").Value = "FUNews Management System - News Statistics Report";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Cell("A1").Style.Font.FontColor = XLColor.DarkBlue;

        string periodText = $"Period: {(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "All Past")} to {(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "Present")}";
        ws.Cell("A2").Value = $"{periodText} | Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}";
        ws.Cell("A2").Style.Font.Italic = true;
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;

        // KPI Summary Block
        ws.Cell("A4").Value = "METRIC";
        ws.Cell("B4").Value = "COUNT";
        ws.Range("A4:B4").Style.Font.Bold = true;
        ws.Range("A4:B4").Style.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell("A5").Value = "Total Articles";
        ws.Cell("B5").Value = report.TotalArticles;
        ws.Cell("A6").Value = "Active (Published) Articles";
        ws.Cell("B6").Value = report.ActiveArticles;
        ws.Cell("A7").Value = "Inactive (Draft) Articles";
        ws.Cell("B7").Value = report.InactiveArticles;
        ws.Range("A4:B7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range("A4:B7").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Category Breakdown Table
        int row = 9;
        ws.Cell(row, 1).Value = "CATEGORY BREAKDOWN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 13;
        row++;

        ws.Cell(row, 1).Value = "Category ID";
        ws.Cell(row, 2).Value = "Category Name";
        ws.Cell(row, 3).Value = "Total Articles";
        ws.Cell(row, 4).Value = "Active Articles";
        ws.Cell(row, 5).Value = "Inactive Articles";
        ws.Range(row, 1, row, 5).Style.Font.Bold = true;
        ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.AliceBlue;
        row++;

        foreach (var cat in report.CategoryGroups)
        {
            ws.Cell(row, 1).Value = cat.CategoryId?.ToString() ?? "N/A";
            ws.Cell(row, 2).Value = cat.CategoryName;
            ws.Cell(row, 3).Value = cat.ArticleCount;
            ws.Cell(row, 4).Value = cat.ActiveCount;
            ws.Cell(row, 5).Value = cat.InactiveCount;
            row++;
        }
        ws.Range(10, 1, row - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(10, 1, row - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Author Breakdown Table
        row += 2;
        ws.Cell(row, 1).Value = "AUTHOR BREAKDOWN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 13;
        row++;

        ws.Cell(row, 1).Value = "Author ID";
        ws.Cell(row, 2).Value = "Author Name";
        ws.Cell(row, 3).Value = "Total Articles";
        ws.Cell(row, 4).Value = "Active Articles";
        ws.Cell(row, 5).Value = "Inactive Articles";
        ws.Range(row, 1, row, 5).Style.Font.Bold = true;
        ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.AliceBlue;
        int authorHeaderRow = row;
        row++;

        foreach (var auth in report.AuthorGroups)
        {
            ws.Cell(row, 1).Value = auth.AuthorId?.ToString() ?? "N/A";
            ws.Cell(row, 2).Value = auth.AuthorName;
            ws.Cell(row, 3).Value = auth.ArticleCount;
            ws.Cell(row, 4).Value = auth.ActiveCount;
            ws.Cell(row, 5).Value = auth.InactiveCount;
            row++;
        }
        ws.Range(authorHeaderRow, 1, row - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(authorHeaderRow, 1, row - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Detailed Articles Table
        row += 2;
        ws.Cell(row, 1).Value = "DETAILED NEWS ARTICLES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 13;
        row++;

        ws.Cell(row, 1).Value = "Article ID";
        ws.Cell(row, 2).Value = "News Title";
        ws.Cell(row, 3).Value = "Category";
        ws.Cell(row, 4).Value = "Author";
        ws.Cell(row, 5).Value = "Status";
        ws.Cell(row, 6).Value = "Created Date";
        ws.Cell(row, 7).Value = "Source";
        ws.Range(row, 1, row, 7).Style.Font.Bold = true;
        ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
        int articleHeaderRow = row;
        row++;

        foreach (var art in report.Articles)
        {
            ws.Cell(row, 1).Value = art.NewsArticleId;
            ws.Cell(row, 2).Value = art.NewsTitle;
            ws.Cell(row, 3).Value = art.CategoryName;
            ws.Cell(row, 4).Value = art.CreatedByName;
            ws.Cell(row, 5).Value = art.NewsStatus == true ? "Active" : "Inactive";
            ws.Cell(row, 6).Value = art.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
            ws.Cell(row, 7).Value = art.NewsSource ?? "N/A";
            row++;
        }
        ws.Range(articleHeaderRow, 1, row - 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(articleHeaderRow, 1, row - 1, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Auto-fit columns
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
