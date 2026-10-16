using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.DAO;

public class TagDAO : BaseDAO
{
    private static readonly object _lock = new object();
    private static TagDAO? _instance;

    private TagDAO() { }

    public static TagDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TagDAO();
                    }
                }
            }
            return _instance;
        }
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        using var context = CreateDbContext();
        return await context.Tags
            .Include(t => t.NewsArticles)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        using var context = CreateDbContext();
        return await context.Tags
            .Include(t => t.NewsArticles)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TagId == id);
    }

    public async Task<bool> IsTagNameDuplicateAsync(string tagName, int? excludeTagId = null)
    {
        using var context = CreateDbContext();
        var normalized = tagName.Trim().ToLower();
        return await context.Tags.AnyAsync(t => t.TagName != null && 
                                                t.TagName.ToLower() == normalized && 
                                                (!excludeTagId.HasValue || t.TagId != excludeTagId.Value));
    }

    public async Task<bool> IsTagUsedInArticlesAsync(int tagId)
    {
        using var context = CreateDbContext();
        // Since NewsTag is configured as many-to-many junction table in EF Core, checking if any article has this tag
        return await context.NewsArticles.AnyAsync(a => a.Tags.Any(t => t.TagId == tagId));
    }

    public async Task<int> GetNextTagIdAsync()
    {
        using var context = CreateDbContext();
        var maxId = await context.Tags.MaxAsync(t => (int?)t.TagId);
        return (maxId ?? 0) + 1;
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        using var context = CreateDbContext();
        if (tag.TagId == 0)
        {
            tag.TagId = await GetNextTagIdAsync();
        }
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> UpdateAsync(Tag tag)
    {
        using var context = CreateDbContext();
        var existing = await context.Tags.FirstOrDefaultAsync(t => t.TagId == tag.TagId);
        if (existing == null) return null;

        existing.TagName = tag.TagName;
        existing.Note = tag.Note;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var context = CreateDbContext();
        var existing = await context.Tags.FirstOrDefaultAsync(t => t.TagId == id);
        if (existing == null) return false;

        context.Tags.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
