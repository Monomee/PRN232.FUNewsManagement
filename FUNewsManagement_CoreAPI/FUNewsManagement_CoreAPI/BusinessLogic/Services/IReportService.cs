using System;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface IReportService
{
    Task<ReportStatisticsDto> GetReportStatisticsAsync(DateTime? startDate, DateTime? endDate);
    Task<byte[]> ExportReportToExcelAsync(DateTime? startDate, DateTime? endDate);
}
