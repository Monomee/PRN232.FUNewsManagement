using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.DAO;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    public Task<List<NewsArticle>> GetAllNewsArticlesAsync() => NewsArticleDAO.Instance.GetAllAsync();

    public Task<NewsArticle?> GetNewsArticleByIdAsync(string id) => NewsArticleDAO.Instance.GetByIdAsync(id, false);
    public Task<NewsArticle?> GetNewsArticleByIdAsync(string id, bool incrementView) => NewsArticleDAO.Instance.GetByIdAsync(id, incrementView);
    public Task<int> IncrementViewCountAsync(string id) => NewsArticleDAO.Instance.IncrementViewCountAsync(id);

    public Task<NewsArticle> AddNewsArticleAsync(NewsArticle article, List<int>? tagIds) => 
        NewsArticleDAO.Instance.AddAsync(article, tagIds);

    public Task<NewsArticle?> UpdateNewsArticleAsync(NewsArticle article, List<int>? tagIds) => 
        NewsArticleDAO.Instance.UpdateAsync(article, tagIds);

    public Task<bool> DeleteNewsArticleAsync(string id) => NewsArticleDAO.Instance.DeleteAsync(id);

    public Task<NewsArticle?> DuplicateNewsArticleAsync(string sourceId, short currentUserId) => 
        NewsArticleDAO.Instance.DuplicateAsync(sourceId, currentUserId);

    public Task<List<NewsArticle>> GetRelatedNewsArticlesAsync(string currentArticleId, short? categoryId, List<int> tagIds) => 
        NewsArticleDAO.Instance.GetRelatedArticlesAsync(currentArticleId, categoryId, tagIds);
}
