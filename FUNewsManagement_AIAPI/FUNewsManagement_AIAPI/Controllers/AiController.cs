using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FUNewsManagement_AIAPI.DTOs;
using FUNewsManagement_AIAPI.Services;

namespace FUNewsManagement_AIAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiTagSuggestionService _aiService;
    private readonly ILearningCacheService _learningCache;

    public AiController(
        IAiTagSuggestionService aiService,
        ILearningCacheService learningCache)
    {
        _aiService = aiService;
        _learningCache = learningCache;
    }

    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags([FromBody] TagSuggestionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _aiService.SuggestTagsAsync(request.Content);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error suggesting tags: " + ex.Message });
        }
    }

    [HttpPost("learn")]
    public IActionResult LearnTags([FromBody] LearnTagsRequestDto request)
    {
        if (request.Tags == null || request.Tags.Count == 0)
        {
            return BadRequest(new { message = "No tags provided to learn." });
        }

        _learningCache.LearnTags(request.Tags);
        return Ok(new
        {
            message = "Learning cache updated successfully.",
            updatedTags = request.Tags,
            snapshot = _learningCache.GetCacheSnapshot()
        });
    }

    [HttpGet("cache")]
    public IActionResult GetCacheSnapshot()
    {
        return Ok(new
        {
            totalLearnedTags = _learningCache.GetCacheSnapshot().Count,
            cache = _learningCache.GetCacheSnapshot()
        });
    }
}
