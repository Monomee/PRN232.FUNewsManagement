using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface INewsArticleService
{
    Task<List<NewsArticleDto>> GetAllArticlesAsync();
    Task<NewsArticleDto?> GetArticleByIdAsync(string id);
    Task<NewsArticleDto?> GetArticleByIdAsync(string id, bool incrementView);
    Task<int> IncrementViewCountAsync(string id);
    Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto dto, short currentUserId);
    Task<NewsArticleDto> UpdateArticleAsync(string id, UpdateNewsArticleDto dto, short currentUserId);
    Task DeleteArticleAsync(string id);
    Task<NewsArticleDto> DuplicateArticleAsync(string id, short currentUserId);
    Task<List<NewsArticleDto>> GetRelatedArticlesAsync(string id);
}
