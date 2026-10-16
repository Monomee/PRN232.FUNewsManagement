using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FUNewsManagement_AnalyticsAPI.Services;

namespace FUNewsManagement_AnalyticsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public RecommendController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecommendations(string id, [FromQuery] int top = 3)
    {
        try
        {
            var results = await _analyticsService.GetRecommendationsAsync(id, top);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error computing article recommendations: " + ex.Message });
        }
    }
}
