using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FUNewsManagement_AnalyticsAPI.Services;

namespace FUNewsManagement_AnalyticsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] short? categoryId,
        [FromQuery] short? authorId)
    {
        try
        {
            var result = await _analyticsService.GetDashboardAsync(startDate, endDate, categoryId, authorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error calculating analytics: " + ex.Message });
        }
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending([FromQuery] int top = 5)
    {
        try
        {
            var result = await _analyticsService.GetTrendingAsync(top);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching trending articles: " + ex.Message });
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] short? categoryId,
        [FromQuery] short? authorId)
    {
        try
        {
            var ms = new MemoryStream();
            await _analyticsService.ExportExcelToStreamAsync(startDate, endDate, categoryId, authorId, ms);
            ms.Position = 0;
            var fileName = $"FUNews_Analytics_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error streaming Excel report: " + ex.Message });
        }
    }

    private static int _failCount = 0;

    [HttpGet("test-polly")]
    public IActionResult TestPolly()
    {
        _failCount++;
        // 2 lần đầu cố tình trả về lỗi 500 Internal Server Error
        if (_failCount <= 2)
        {
            Console.WriteLine($"[API] Request #{_failCount} -> Trả về 500 lỗi tạm thời (Transient Error).");
            return StatusCode(500, new { message = $"Lỗi tạm thời lần {_failCount} từ Analytics API" });
        }

        // Lần thứ 3 thành công
        Console.WriteLine($"[API] Request #{_failCount} -> Thành công 200 OK!");
        _failCount = 0; // reset
        return Ok(new { message = "Request thành công sau khi Polly Retry!" });
    }
}
