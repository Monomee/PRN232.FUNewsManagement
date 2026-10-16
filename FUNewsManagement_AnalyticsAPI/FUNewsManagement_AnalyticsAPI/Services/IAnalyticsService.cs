using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FUNewsManagement_AnalyticsAPI.DTOs;

namespace FUNewsManagement_AnalyticsAPI.Services;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAsync(DateTime? startDate, DateTime? endDate, short? categoryId, short? authorId);
    Task<List<TrendingArticleDto>> GetTrendingAsync(int top = 5);
    Task<List<RecommendedArticleDto>> GetRecommendationsAsync(string articleId, int top = 3);
    Task ExportExcelToStreamAsync(DateTime? startDate, DateTime? endDate, short? categoryId, short? authorId, Stream outputStream);
}
