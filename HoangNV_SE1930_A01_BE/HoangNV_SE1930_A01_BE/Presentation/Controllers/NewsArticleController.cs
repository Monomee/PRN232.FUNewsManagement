using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/news")]
[Route("api/[controller]")]
public class NewsArticleController : ControllerBase
{
    private readonly INewsArticleService _articleService;

    public NewsArticleController(INewsArticleService articleService)
    {
        _articleService = articleService;
    }
    [HttpGet("my-articles")]
    [Authorize(Roles = "Staff")]
    [EnableQuery] 
    public async Task<IActionResult> GetMyArticles()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!short.TryParse(userIdStr, out short currentUserId))
        {
            return Unauthorized(new { message = "Invalid user token." });
        }

        var allArticles = await _articleService.GetAllArticlesAsync();
        var myArticles = allArticles.Where(a => a.CreatedById == currentUserId).ToList();

        return Ok(myArticles);
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableQuery]
    public async Task<IActionResult> GetNewsArticles()
    {
        var articles = await _articleService.GetAllArticlesAsync();
        return Ok(articles);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNewsArticle(string id)
    {
        var article = await _articleService.GetArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound(new { message = $"News article with ID '{id}' not found." });
        }
        return Ok(article);
    }

    [HttpGet("{id}/related")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRelatedArticles(string id)
    {
        var related = await _articleService.GetRelatedArticlesAsync(id);
        return Ok(related);
    }

    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateNewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            short.TryParse(userIdStr, out short currentUserId);

            var created = await _articleService.CreateArticleAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetNewsArticle), new { id = created.NewsArticleId }, created);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating article: " + ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> UpdateArticle(string id, [FromBody] UpdateNewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            short.TryParse(userIdStr, out short currentUserId);

            var updated = await _articleService.UpdateArticleAsync(id, dto, currentUserId);
            return Ok(updated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating article: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> DeleteArticle(string id)
    {
        try
        {
            await _articleService.DeleteArticleAsync(id);
            return NoContent();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting article: " + ex.Message });
        }
    }

    [HttpPost("{id}/duplicate")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> DuplicateArticle(string id)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            short.TryParse(userIdStr, out short currentUserId);

            var duplicated = await _articleService.DuplicateArticleAsync(id, currentUserId);
            return Ok(duplicated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error duplicating article: " + ex.Message });
        }
    }
}
