using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(int id);
    Task<bool> IsTagNameDuplicateAsync(string tagName, int? excludeTagId = null);
    Task<bool> IsTagUsedInArticlesAsync(int tagId);
    Task<Tag> AddTagAsync(Tag tag);
    Task<Tag?> UpdateTagAsync(Tag tag);
    Task<bool> DeleteTagAsync(int id);
}
