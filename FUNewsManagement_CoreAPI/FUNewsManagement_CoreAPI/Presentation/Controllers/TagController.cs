using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableQuery]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { message = $"Tag with ID {id} not found." });
        }
        return Ok(tag);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _tagService.CreateTagAsync(dto);
            return CreatedAtAction(nameof(GetTag), new { id = created.TagId }, created);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating tag: " + ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _tagService.UpdateTagAsync(id, dto);
            return Ok(updated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating tag: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            return NoContent();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting tag: " + ex.Message });
        }
    }
}
