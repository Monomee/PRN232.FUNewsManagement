using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.DAO;

public class CategoryDAO : BaseDAO
{
    private static readonly object _lock = new object();
    private static CategoryDAO? _instance;

    private CategoryDAO() { }

    public static CategoryDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CategoryDAO();
                    }
                }
            }
            return _instance;
        }
    }

    public async Task<List<Category>> GetAllAsync()
    {
        using var context = CreateDbContext();
        return await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.NewsArticles)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(short id)
    {
        using var context = CreateDbContext();
        return await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.NewsArticles)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task<bool> IsSubCategoryNameDuplicateAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null)
    {
        using var context = CreateDbContext();
        var normalizedName = categoryName.Trim().ToLower();
        return await context.Categories.AnyAsync(c => 
            c.ParentCategoryId == parentCategoryId &&
            c.CategoryName.ToLower() == normalizedName &&
            (!excludeCategoryId.HasValue || c.CategoryId != excludeCategoryId.Value));
    }

    public async Task<bool> HasNewsArticlesAsync(short categoryId)
    {
        using var context = CreateDbContext();
        return await context.NewsArticles.AnyAsync(n => n.CategoryId == categoryId);
    }

    public async Task<short> GetNextCategoryIdAsync()
    {
        using var context = CreateDbContext();
        var maxId = await context.Categories.MaxAsync(c => (short?)c.CategoryId);
        return (short)((maxId ?? 0) + 1);
    }

    public async Task<Category> AddAsync(Category category)
    {
        using var context = CreateDbContext();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        using var context = CreateDbContext();
        var existing = await context.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);
        if (existing == null) return null;

        existing.CategoryName = category.CategoryName;
        existing.CategoryDesciption = category.CategoryDesciption;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(short id)
    {
        using var context = CreateDbContext();
        var existing = await context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (existing == null) return false;

        context.Categories.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
