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
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableQuery]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(short id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found." });
        }
        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { id = created.CategoryId }, created);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating category: " + ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateCategory(short id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _categoryService.UpdateCategoryAsync(id, dto);
            return Ok(updated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating category: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteCategory(short id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting category: " + ex.Message });
        }
    }
}
