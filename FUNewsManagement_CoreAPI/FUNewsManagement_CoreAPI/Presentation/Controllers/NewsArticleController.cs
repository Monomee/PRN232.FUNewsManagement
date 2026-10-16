using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/news")]
[Route("api/[controller]")]
public class NewsArticleController : ControllerBase
{
    private readonly INewsArticleService _articleService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public NewsArticleController(
        INewsArticleService articleService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _articleService = articleService;
        _environment = environment;
        _configuration = configuration;
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

    [HttpPost("{id}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> IncrementViewCount(string id)
    {
        var count = await _articleService.IncrementViewCountAsync(id);
        return Ok(new { newsArticleId = id, viewCount = count });
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

    [HttpPost("upload-image")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No image file was uploaded." });
        }

        // Validate max size: 2MB (2 * 1024 * 1024)
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds the 2MB limit." });
        }

        // Validate file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp." });
        }

        // Validate MIME type
        var allowedMimes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedMimes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Invalid MIME type. Must be image/jpeg, image/png, or image/webp." });
        }

        try
        {
            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);
                cloudinary.Api.Secure = true;

                await using var uploadStream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, uploadStream),
                    Folder = "funews_articles"
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    return StatusCode(500, new { message = "Cloudinary upload failed: " + uploadResult.Error.Message });
                }

                var secureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                return Ok(new { imageUrl = secureUrl });
            }

            // Fallback to local uploads folder
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var absoluteUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
            return Ok(new { imageUrl = absoluteUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload image: " + ex.Message });
        }
    }
}
