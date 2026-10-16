using System;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
}

public class AuditLogPagedResultDto
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
