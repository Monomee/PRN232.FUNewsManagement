using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.DAO;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public class TagRepository : ITagRepository
{
    public Task<List<Tag>> GetAllTagsAsync() => TagDAO.Instance.GetAllAsync();

    public Task<Tag?> GetTagByIdAsync(int id) => TagDAO.Instance.GetByIdAsync(id);

    public Task<bool> IsTagNameDuplicateAsync(string tagName, int? excludeTagId = null) =>
        TagDAO.Instance.IsTagNameDuplicateAsync(tagName, excludeTagId);

    public Task<bool> IsTagUsedInArticlesAsync(int tagId) => TagDAO.Instance.IsTagUsedInArticlesAsync(tagId);

    public Task<Tag> AddTagAsync(Tag tag) => TagDAO.Instance.AddAsync(tag);

    public Task<Tag?> UpdateTagAsync(Tag tag) => TagDAO.Instance.UpdateAsync(tag);

    public Task<bool> DeleteTagAsync(int id) => TagDAO.Instance.DeleteAsync(id);
}
