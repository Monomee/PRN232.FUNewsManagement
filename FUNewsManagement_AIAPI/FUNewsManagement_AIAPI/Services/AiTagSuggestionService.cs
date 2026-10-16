using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FUNewsManagement_AIAPI.DTOs;

namespace FUNewsManagement_AIAPI.Services;

public interface IAiTagSuggestionService
{
    Task<TagSuggestionResponseDto> SuggestTagsAsync(string content);
}

public class AiTagSuggestionService : IAiTagSuggestionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILearningCacheService _learningCache;
    private readonly ILogger<AiTagSuggestionService> _logger;

    public AiTagSuggestionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILearningCacheService learningCache,
        ILogger<AiTagSuggestionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _learningCache = learningCache;
        _logger = logger;
    }

    public async Task<TagSuggestionResponseDto> SuggestTagsAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new TagSuggestionResponseDto { Tags = new List<TagSuggestionDto>() };
        }

        // 1. Google AI Studio (Gemini)
        var googleApiKey = _configuration["GoogleAI:ApiKey"]?.Trim();
        var googleModel = _configuration["GoogleAI:Model"]?.Trim() ?? "gemini-3.5-flash-lite";

        if (!string.IsNullOrEmpty(googleApiKey))
        {
            try
            {
                var response = await CallGoogleAiAsync(googleApiKey, googleModel, content);
                if (response != null && response.Tags.Count > 0)
                {
                    ApplyLearningCacheBoosts(response);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Google AI Studio call failed: {Message}. Checking OpenAI fallback.", ex.Message);
            }
        }

        // 2. OpenAI
        var openAiKey = _configuration["OpenAI:ApiKey"]?.Trim();
        var openAiModel = _configuration["OpenAI:Model"]?.Trim() ?? "gpt-4o-mini";

        if (!string.IsNullOrEmpty(openAiKey))
        {
            try
            {
                var response = await CallOpenAiAsync(openAiKey, openAiModel, content);
                if (response != null && response.Tags.Count > 0)
                {
                    ApplyLearningCacheBoosts(response);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("OpenAI API call failed: {Message}. Falling back to internal NLP heuristic extraction.", ex.Message);
            }
        }

        // 3. Fallback keyword extraction with Learning Cache boosting
        return GenerateFallbackSuggestions(content);
    }

    private void ApplyLearningCacheBoosts(TagSuggestionResponseDto response)
    {
        foreach (var tag in response.Tags)
        {
            var boost = _learningCache.GetBoost(tag.Name);
            tag.Confidence = Math.Min(0.99, Math.Round(tag.Confidence + boost, 2));
        }

        response.Tags = response.Tags.OrderByDescending(t => t.Confidence).ToList();
    }

    private static readonly HashSet<string> EnglishStopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't",
        "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
        "can", "can't", "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing",
        "don't", "down", "during", "each", "few", "for", "from", "further", "had", "hadn't", "has", "hasn't",
        "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself",
        "him", "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is",
        "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most", "mustn't", "my", "myself",
        "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves",
        "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't",
        "so", "some", "such", "than", "that", "that's", "the", "their", "theirs", "them", "themselves",
        "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this",
        "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasn't", "we", "we'd", "we'll",
        "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which",
        "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't", "you", "you'd",
        "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
    };

    private async Task<TagSuggestionResponseDto?> CallGoogleAiAsync(string apiKey, string model, string content)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var prompt = @"You are an intelligent news tagging engine. 
Analyze the provided news article content and recommend 3 to 5 relevant tags/topics.
CRITICAL INSTRUCTIONS:
1. All tag names MUST be in ENGLISH ONLY (regardless of whether the input article is in Vietnamese or another language).
2. Format tag names in Title Case (e.g., 'Artificial Intelligence', 'Economy', 'Cloud Computing', 'Higher Education').
3. Do NOT use Vietnamese or local language tags.
Return ONLY valid JSON matching this exact structure:
{
  ""tags"": [
    { ""name"": ""TagNameInEnglish"", ""confidence"": 0.85 }
  ]
}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"{prompt}\n\nContent:\n{(content.Length > 2500 ? content.Substring(0, 2500) : content)}" }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.3
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Google AI Studio returned status {StatusCode}: {Error}", response.StatusCode, err);
            return null;
        }

        var resJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(resJson);
        var candidates = doc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0) return null;

        var parts = candidates[0].GetProperty("content").GetProperty("parts");
        if (parts.GetArrayLength() == 0) return null;

        var rawJsonText = parts[0].GetProperty("text").GetString();
        if (string.IsNullOrWhiteSpace(rawJsonText)) return null;

        var result = JsonSerializer.Deserialize<TagSuggestionResponseDto>(rawJsonText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result != null)
        {
            result.Source = $"Google AI Studio ({model})";
        }

        return result;
    }

    private async Task<TagSuggestionResponseDto?> CallOpenAiAsync(string apiKey, string model, string content)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = @"You are an intelligent news tagging engine. 
Analyze the provided news article content and recommend 3 to 5 relevant tags/topics.
CRITICAL INSTRUCTIONS:
1. All tag names MUST be in ENGLISH ONLY (regardless of whether the input article is in Vietnamese or another language).
2. Format tag names in Title Case (e.g., 'Artificial Intelligence', 'Economy', 'Cloud Computing', 'Higher Education').
3. Do NOT use Vietnamese or local language tags.
Return ONLY valid JSON matching this exact structure:
{
  ""tags"": [
    { ""name"": ""TagNameInEnglish"", ""confidence"": 0.85 }
  ]
}";

        var requestBody = new
        {
            model = model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = content.Length > 2500 ? content.Substring(0, 2500) : content }
            },
            temperature = 0.3
        };

        var json = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("OpenAI returned status {StatusCode}: {Error}", response.StatusCode, err);
            return null;
        }

        var resJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(resJson);
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0) return null;

        var messageContent = choices[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrEmpty(messageContent)) return null;

        var result = JsonSerializer.Deserialize<TagSuggestionResponseDto>(messageContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result != null)
        {
            result.Source = $"OpenAI ({model})";
        }

        return result;
    }

    private TagSuggestionResponseDto GenerateFallbackSuggestions(string content)
    {
        var domainVocabulary = new Dictionary<string, string[]>
        {
            { "Artificial Intelligence", new[] { "ai", "machine learning", "deep learning", "algorithm", "model", "neural", "robotics", "tri tue nhan tao", "hoc may" } },
            { "Technology", new[] { "software", "tech", "computer", "cloud", "code", "programming", "system", "digital", "data", "cong nghe", "phan mem" } },
            { "Higher Education", new[] { "student", "faculty", "curriculum", "course", "degree", "academic", "university", "college", "school", "dai hoc", "sinh vien", "giang vien" } },
            { "Scientific Research", new[] { "research", "study", "findings", "scientific", "investigation", "lab", "discovery", "experiment", "nghien cuu", "khoa hoc" } },
            { "Innovation", new[] { "innovation", "innovative", "novel", "groundbreaking", "future", "transform", "creative", "startup", "doi moi", "sang tao" } },
            { "Economy & Business", new[] { "economy", "market", "business", "finance", "investment", "trade", "enterprise", "kinh te", "kinh doanh", "tai chinh" } },
            { "Cloud Computing", new[] { "cloud", "aws", "azure", "serverless", "devops", "microservices", "dien toan dam may" } },
            { "Cybersecurity", new[] { "security", "cyber", "protection", "firewall", "hacker", "encryption", "bao mat", "an ninh mang" } },
            { "Alumni & Career", new[] { "alumni", "graduate", "career", "success", "network", "mentor", "mentorship", "job", "hiring", "cuu sinh vien", "viec lam" } },
            { "Campus Life", new[] { "campus", "club", "student life", "event", "activities", "sports", "community", "hoi sinh vien", "clb" } }
        };

        var contentLower = content.ToLowerInvariant();
        var suggestedList = new List<TagSuggestionDto>();

        foreach (var (tag, keywords) in domainVocabulary)
        {
            int matches = 0;
            foreach (var kw in keywords)
            {
                if (Regex.IsMatch(contentLower, $@"\b{Regex.Escape(kw)}\b"))
                {
                    matches++;
                }
            }

            if (matches > 0)
            {
                double baseConf = Math.Min(0.88, 0.55 + (matches * 0.10));
                double boost = _learningCache.GetBoost(tag);
                suggestedList.Add(new TagSuggestionDto
                {
                    Name = tag,
                    Confidence = Math.Min(0.99, Math.Round(baseConf + boost, 2))
                });
            }
        }

        // Tokenize English words and filter stopwords
        var wordMatches = Regex.Matches(content, @"\b[A-Za-z]{4,}\b");
        var wordFreq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in wordMatches)
        {
            var word = match.Value;
            if (!EnglishStopwords.Contains(word))
            {
                wordFreq[word] = wordFreq.GetValueOrDefault(word, 0) + 1;
            }
        }

        // Add top English keywords if domain vocabulary returned < 3
        if (suggestedList.Count < 3)
        {
            foreach (var kvp in wordFreq.OrderByDescending(k => k.Value).Take(5))
            {
                var titleCased = char.ToUpperInvariant(kvp.Key[0]) + kvp.Key.Substring(1).ToLowerInvariant();
                if (!suggestedList.Any(t => t.Name.Equals(titleCased, StringComparison.OrdinalIgnoreCase)))
                {
                    double baseConf = Math.Min(0.80, 0.50 + (kvp.Value * 0.05));
                    suggestedList.Add(new TagSuggestionDto
                    {
                        Name = titleCased,
                        Confidence = Math.Min(0.95, Math.Round(baseConf, 2))
                    });
                }
            }
        }

        // If still empty, supply default learned tags
        if (suggestedList.Count == 0)
        {
            var learnedSnapshot = _learningCache.GetCacheSnapshot();
            foreach (var kvp in learnedSnapshot.OrderByDescending(k => k.Value).Take(3))
            {
                suggestedList.Add(new TagSuggestionDto
                {
                    Name = kvp.Key,
                    Confidence = 0.60
                });
            }
        }

        return new TagSuggestionResponseDto
        {
            Tags = suggestedList.OrderByDescending(t => t.Confidence).Take(5).ToList(),
            Source = "NLP-Heuristic (Offline Resilience)"
        };
    }
}
