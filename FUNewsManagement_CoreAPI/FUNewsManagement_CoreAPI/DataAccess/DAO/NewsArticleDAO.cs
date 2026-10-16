using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.DAO;

public class NewsArticleDAO : BaseDAO
{
    private static readonly object _lock = new object();
    private static NewsArticleDAO? _instance;

    private NewsArticleDAO() { }

    public static NewsArticleDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new NewsArticleDAO();
                    }
                }
            }
            return _instance;
        }
    }

    public async Task<List<NewsArticle>> GetAllAsync()
    {
        using var context = CreateDbContext();
        return await context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> IncrementViewCountAsync(string id)
    {
        using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE NewsArticle SET ViewCount = ViewCount + 1 WHERE NewsArticleID = {id}");
        var article = await context.NewsArticles.AsNoTracking().FirstOrDefaultAsync(n => n.NewsArticleId == id);
        return article?.ViewCount ?? 0;
    }

    public async Task<NewsArticle?> GetByIdAsync(string id, bool incrementView = false)
    {
        using var context = CreateDbContext();
        if (incrementView)
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE NewsArticle SET ViewCount = ViewCount + 1 WHERE NewsArticleID = {id}");
        }
        return await context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
    }

    public async Task<string> GenerateNextArticleIdAsync()
    {
        using var context = CreateDbContext();
        var ids = await context.NewsArticles.Select(n => n.NewsArticleId).ToListAsync();
        int maxNumeric = 0;
        foreach (var id in ids)
        {
            if (int.TryParse(id, out int num) && num > maxNumeric)
            {
                maxNumeric = num;
            }
        }
        return (maxNumeric + 1).ToString();
    }

    public async Task<NewsArticle> AddAsync(NewsArticle article, List<int>? tagIds)
    {
        using var context = CreateDbContext();
        if (string.IsNullOrWhiteSpace(article.NewsArticleId))
        {
            article.NewsArticleId = await GenerateNextArticleIdAsync();
        }

        if (tagIds != null && tagIds.Count > 0)
        {
            var selectedTags = await context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
            foreach (var tag in selectedTags)
            {
                article.Tags.Add(tag);
            }
        }

        await context.NewsArticles.AddAsync(article);
        await context.SaveChangesAsync();
        return article;
    }

    public async Task<NewsArticle?> UpdateAsync(NewsArticle article, List<int>? tagIds)
    {
        using var context = CreateDbContext();
        var existing = await context.NewsArticles
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == article.NewsArticleId);
        
        if (existing == null) return null;

        existing.NewsTitle = article.NewsTitle;
        existing.Headline = article.Headline;
        existing.NewsContent = article.NewsContent;
        existing.NewsSource = article.NewsSource;
        existing.CategoryId = article.CategoryId;
        existing.NewsStatus = article.NewsStatus;
        existing.UpdatedById = article.UpdatedById;
        existing.ModifiedDate = article.ModifiedDate ?? DateTime.Now;
        existing.ImageUrl = article.ImageUrl;

        if (tagIds != null)
        {
            // Sync tags
            existing.Tags.Clear();
            var newTags = await context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
            foreach (var tag in newTags)
            {
                existing.Tags.Add(tag);
            }
        }

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var context = CreateDbContext();
        var existing = await context.NewsArticles
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        
        if (existing == null) return false;

        // Clear junction tags first
        existing.Tags.Clear();
        await context.SaveChangesAsync();

        context.NewsArticles.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<NewsArticle?> DuplicateAsync(string sourceId, short currentUserId)
    {
        using var context = CreateDbContext();
        var source = await context.NewsArticles
            .Include(n => n.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.NewsArticleId == sourceId);
        
        if (source == null) return null;

        var newId = await GenerateNextArticleIdAsync();
        var duplicated = new NewsArticle
        {
            NewsArticleId = newId,
            NewsTitle = "[Copy] " + source.NewsTitle,
            Headline = source.Headline,
            NewsContent = source.NewsContent,
            NewsSource = source.NewsSource,
            CategoryId = source.CategoryId,
            NewsStatus = source.NewsStatus,
            CreatedById = currentUserId,
            CreatedDate = DateTime.Now,
            UpdatedById = null,
            ModifiedDate = null
        };

        if (source.Tags != null && source.Tags.Count > 0)
        {
            var tagIds = source.Tags.Select(t => t.TagId).ToList();
            var tagsToAttach = await context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
            foreach (var tag in tagsToAttach)
            {
                duplicated.Tags.Add(tag);
            }
        }

        await context.NewsArticles.AddAsync(duplicated);
        await context.SaveChangesAsync();
        return duplicated;
    }

    public async Task<List<NewsArticle>> GetRelatedArticlesAsync(string currentArticleId, short? categoryId, List<int> tagIds)
    {
        using var context = CreateDbContext();
        // Top 3 related active articles:
        // n.CategoryID == CurrentCategoryID OR shares at least 1 tag, and n.NewsArticleID != CurrentNewsID, and NewsStatus == true
        var query = context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .Include(n => n.CreatedBy)
            .Where(n => n.NewsArticleId != currentArticleId && n.NewsStatus == true);

        if (categoryId.HasValue && tagIds.Count > 0)
        {
            query = query.Where(n => n.CategoryId == categoryId.Value || n.Tags.Any(t => tagIds.Contains(t.TagId)));
        }
        else if (categoryId.HasValue)
        {
            query = query.Where(n => n.CategoryId == categoryId.Value);
        }
        else if (tagIds.Count > 0)
        {
            query = query.Where(n => n.Tags.Any(t => tagIds.Contains(t.TagId)));
        }

        return await query
            .OrderByDescending(n => n.CreatedDate)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();
    }
}
