using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_AIAPI.DTOs;

public class TagSuggestionRequestDto
{
    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;
}

public class TagSuggestionDto
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class TagSuggestionResponseDto
{
    public List<TagSuggestionDto> Tags { get; set; } = new();
    public string Source { get; set; } = "OpenAI";
}

public class LearnTagsRequestDto
{
    [Required(ErrorMessage = "Tags array is required")]
    public List<string> Tags { get; set; } = new();
}
