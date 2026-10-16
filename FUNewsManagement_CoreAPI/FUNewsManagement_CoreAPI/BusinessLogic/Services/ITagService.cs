using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface ITagService
{
    Task<List<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(int id);
    Task<TagDto> CreateTagAsync(CreateTagDto dto);
    Task<TagDto> UpdateTagAsync(int id, UpdateTagDto dto);
    Task DeleteTagAsync(int id);
}
