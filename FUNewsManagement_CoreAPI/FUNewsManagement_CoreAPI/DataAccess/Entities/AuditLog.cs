using System;

namespace HoangNV_SE1930_A01_BE.DataAccess.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
}
