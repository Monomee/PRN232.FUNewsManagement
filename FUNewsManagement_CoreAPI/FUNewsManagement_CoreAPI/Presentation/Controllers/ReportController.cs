using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var stats = await _reportService.GetReportStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating statistics report: " + ex.Message });
        }
    }

    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var excelBytes = await _reportService.ExportReportToExcelAsync(startDate, endDate);
            var filename = $"FUNews_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error exporting report to Excel: " + ex.Message });
        }
    }
}
