using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.DataAccess.Context;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly FunewsManagementContext _context;

    public AuditLogController(FunewsManagementContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? userId,
        [FromQuery] string? entityName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId != null && a.UserId.Contains(userId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName.Contains(entityName.Trim()));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                EntityName = a.EntityName,
                Timestamp = a.Timestamp,
                BeforeJson = a.BeforeJson,
                AfterJson = a.AfterJson
            })
            .ToListAsync();

        return Ok(new AuditLogPagedResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
}
