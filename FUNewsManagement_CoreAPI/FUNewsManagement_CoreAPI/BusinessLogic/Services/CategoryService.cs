using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();

        return categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryDesciption = c.CategoryDesciption,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.CategoryName,
            IsActive = c.IsActive,
            ArticleCount = c.NewsArticles?.Count ?? 0
        }).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(short id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null) return null;

        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryDesciption = category.CategoryDesciption,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.CategoryName,
            IsActive = category.IsActive,
            ArticleCount = category.NewsArticles?.Count ?? 0
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.IsSubCategoryNameDuplicateAsync(dto.CategoryName, dto.ParentCategoryId))
        {
            throw new BusinessRuleException($"Category name '{dto.CategoryName}' already exists under the selected parent category.");
        }

        var category = new Category
        {
            CategoryName = dto.CategoryName.Trim(),
            CategoryDesciption = dto.CategoryDesciption.Trim(),
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive ?? true
        };

        var created = await _categoryRepository.AddCategoryAsync(category);

        return new CategoryDto
        {
            CategoryId = created.CategoryId,
            CategoryName = created.CategoryName,
            CategoryDesciption = created.CategoryDesciption,
            ParentCategoryId = created.ParentCategoryId,
            IsActive = created.IsActive,
            ArticleCount = 0
        };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(short id, UpdateCategoryDto dto)
    {
        var existing = await _categoryRepository.GetCategoryByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Category with ID {id} does not exist.");
        }

        // Check duplicate name under parent
        if (await _categoryRepository.IsSubCategoryNameDuplicateAsync(dto.CategoryName, dto.ParentCategoryId, id))
        {
            throw new BusinessRuleException($"Category name '{dto.CategoryName}' already exists under the selected parent category.");
        }

        // Rule: When editing, ParentCategoryID cannot be changed if the category is already used by articles.
        if (existing.ParentCategoryId != dto.ParentCategoryId)
        {
            if (await _categoryRepository.HasNewsArticlesAsync(id))
            {
                throw new BusinessRuleException($"Cannot change the parent category because category '{existing.CategoryName}' is already associated with existing news articles.");
            }
        }

        existing.CategoryName = dto.CategoryName.Trim();
        existing.CategoryDesciption = dto.CategoryDesciption.Trim();
        existing.ParentCategoryId = dto.ParentCategoryId;
        existing.IsActive = dto.IsActive;

        var updated = await _categoryRepository.UpdateCategoryAsync(existing);

        return new CategoryDto
        {
            CategoryId = updated!.CategoryId,
            CategoryName = updated.CategoryName,
            CategoryDesciption = updated.CategoryDesciption,
            ParentCategoryId = updated.ParentCategoryId,
            IsActive = updated.IsActive,
            ArticleCount = updated.NewsArticles?.Count ?? 0
        };
    }

    public async Task DeleteCategoryAsync(short id)
    {
        var existing = await _categoryRepository.GetCategoryByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Category with ID {id} does not exist.");
        }

        // Rule: The category may be removed only if no records exist in NewsArticle.CategoryID.
        if (await _categoryRepository.HasNewsArticlesAsync(id))
        {
            throw new BusinessRuleException($"Cannot delete category '{existing.CategoryName}' (ID: {id}) because it contains associated news articles.");
        }

        await _categoryRepository.DeleteCategoryAsync(id);
    }
}
