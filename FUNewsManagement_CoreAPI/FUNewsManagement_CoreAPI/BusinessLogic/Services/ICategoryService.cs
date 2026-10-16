using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(short id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(short id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(short id);
}
