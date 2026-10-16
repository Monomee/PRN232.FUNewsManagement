using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public interface INewsArticleRepository
{
    Task<List<NewsArticle>> GetAllNewsArticlesAsync();
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id, bool incrementView);
    Task<int> IncrementViewCountAsync(string id);
    Task<NewsArticle> AddNewsArticleAsync(NewsArticle article, List<int>? tagIds);
    Task<NewsArticle?> UpdateNewsArticleAsync(NewsArticle article, List<int>? tagIds);
    Task<bool> DeleteNewsArticleAsync(string id);
    Task<NewsArticle?> DuplicateNewsArticleAsync(string sourceId, short currentUserId);
    Task<List<NewsArticle>> GetRelatedNewsArticlesAsync(string currentArticleId, short? categoryId, List<int> tagIds);
}
