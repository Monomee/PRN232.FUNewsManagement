using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.DAO;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    public Task<List<Category>> GetAllCategoriesAsync() => CategoryDAO.Instance.GetAllAsync();

    public Task<Category?> GetCategoryByIdAsync(short id) => CategoryDAO.Instance.GetByIdAsync(id);

    public Task<bool> IsSubCategoryNameDuplicateAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null) =>
        CategoryDAO.Instance.IsSubCategoryNameDuplicateAsync(categoryName, parentCategoryId, excludeCategoryId);

    public Task<bool> HasNewsArticlesAsync(short categoryId) => CategoryDAO.Instance.HasNewsArticlesAsync(categoryId);

    public Task<Category> AddCategoryAsync(Category category) => CategoryDAO.Instance.AddAsync(category);

    public Task<Category?> UpdateCategoryAsync(Category category) => CategoryDAO.Instance.UpdateAsync(category);

    public Task<bool> DeleteCategoryAsync(short id) => CategoryDAO.Instance.DeleteAsync(id);
}
