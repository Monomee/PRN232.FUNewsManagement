using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FUNewsManagement_AIAPI.Services;

public interface ILearningCacheService
{
    void LearnTags(IEnumerable<string> tags);
    double GetBoost(string tagName);
    IReadOnlyDictionary<string, int> GetCacheSnapshot();
}

public class LearningCacheService : ILearningCacheService
{
    private readonly ConcurrentDictionary<string, int> _cache = new(System.StringComparer.OrdinalIgnoreCase);

    public LearningCacheService()
    {
        // Seed common tags with initial baseline learning data
        _cache.TryAdd("Technology", 5);
        _cache.TryAdd("Education", 4);
        _cache.TryAdd("Innovation", 3);
        _cache.TryAdd("Alumni", 3);
        _cache.TryAdd("Research", 3);
    }

    public void LearnTags(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag)) continue;
            var clean = tag.Trim();
            _cache.AddOrUpdate(clean, 1, (_, current) => current + 1);
        }
    }

    public double GetBoost(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return 0;
        if (_cache.TryGetValue(tagName.Trim(), out int count))
        {
            // Each historical selection boosts confidence by 0.05, max +0.25
            return System.Math.Min(0.25, count * 0.05);
        }
        return 0;
    }

    public IReadOnlyDictionary<string, int> GetCacheSnapshot()
    {
        return new Dictionary<string, int>(_cache);
    }
}
