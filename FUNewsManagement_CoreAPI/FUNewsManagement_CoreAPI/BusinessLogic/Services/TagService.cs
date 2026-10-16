using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        var tags = await _tagRepository.GetAllTagsAsync();
        return tags.Select(t => new TagDto
        {
            TagId = t.TagId,
            TagName = t.TagName,
            Note = t.Note,
            ArticleCount = t.NewsArticles?.Count ?? 0
        }).ToList();
    }

    public async Task<TagDto?> GetTagByIdAsync(int id)
    {
        var tag = await _tagRepository.GetTagByIdAsync(id);
        if (tag == null) return null;

        return new TagDto
        {
            TagId = tag.TagId,
            TagName = tag.TagName,
            Note = tag.Note,
            ArticleCount = tag.NewsArticles?.Count ?? 0
        };
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto dto)
    {
        if (await _tagRepository.IsTagNameDuplicateAsync(dto.TagName))
        {
            throw new BusinessRuleException($"Tag with name '{dto.TagName}' already exists.");
        }

        var tag = new Tag
        {
            TagName = dto.TagName.Trim(),
            Note = dto.Note?.Trim()
        };

        var created = await _tagRepository.AddTagAsync(tag);

        return new TagDto
        {
            TagId = created.TagId,
            TagName = created.TagName,
            Note = created.Note,
            ArticleCount = 0
        };
    }

    public async Task<TagDto> UpdateTagAsync(int id, UpdateTagDto dto)
    {
        var existing = await _tagRepository.GetTagByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Tag with ID {id} does not exist.");
        }

        if (await _tagRepository.IsTagNameDuplicateAsync(dto.TagName, id))
        {
            throw new BusinessRuleException($"Tag with name '{dto.TagName}' already exists.");
        }

        existing.TagName = dto.TagName.Trim();
        existing.Note = dto.Note?.Trim();

        var updated = await _tagRepository.UpdateTagAsync(existing);

        return new TagDto
        {
            TagId = updated!.TagId,
            TagName = updated.TagName,
            Note = updated.Note,
            ArticleCount = updated.NewsArticles?.Count ?? 0
        };
    }

    public async Task DeleteTagAsync(int id)
    {
        var existing = await _tagRepository.GetTagByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Tag with ID {id} does not exist.");
        }

        // Rule: A tag cannot be deleted if it is referenced in NewsTag
        if (await _tagRepository.IsTagUsedInArticlesAsync(id))
        {
            throw new BusinessRuleException($"Cannot delete tag '{existing.TagName}' (ID: {id}) because it is assigned to existing news articles.");
        }

        await _tagRepository.DeleteTagAsync(id);
    }
}
