using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(short id);
    Task<bool> IsSubCategoryNameDuplicateAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null);
    Task<bool> HasNewsArticlesAsync(short categoryId);
    Task<Category> AddCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(short id);
}
