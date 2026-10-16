using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FUNewsManagement_FE.BackgroundServices;

public class DataCacheRefreshWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DataCacheRefreshWorker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(6);

    public DataCacheRefreshWorker(
        IServiceScopeFactory scopeFactory,
        IMemoryCache memoryCache,
        ILogger<DataCacheRefreshWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataCacheRefreshWorker Background Service started. Running every 6 hours.");

        // Initial warm-up run on startup
        try
        {
            await RefreshDataCacheAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Initial data cache warm-up encountered an error: {Message}", ex.Message);
        }

        using var timer = new PeriodicTimer(_period);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Periodic 6-hour cache refresh triggered at {Time}", DateTime.UtcNow);
                await RefreshDataCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cached data in background worker.");
            }
        }
    }

    public async Task RefreshDataCacheAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var clientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var coreClient = clientFactory.CreateClient("CoreApiClient");
        var analyticsClient = clientFactory.CreateClient("AnalyticsApiClient");

        try
        {
            // 1. Fetch & Cache Categories
            var catRes = await coreClient.GetAsync("category", cancellationToken);
            if (catRes.IsSuccessStatusCode)
            {
                var catJson = await catRes.Content.ReadAsStringAsync(cancellationToken);
                _memoryCache.Set("CachedCategories", catJson, TimeSpan.FromHours(7));
                SaveSnapshotFile("categories.json", catJson);
            }

            // 2. Fetch & Cache News (TOP 10 Newest with Full Content)
            var newsRes = await coreClient.GetAsync("news", cancellationToken);
            if (newsRes.IsSuccessStatusCode)
            {
                var newsJson = await newsRes.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    using var doc = JsonDocument.Parse(newsJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var articles = doc.RootElement.EnumerateArray().ToList();
                        var top10 = articles
                            .OrderByDescending(a =>
                            {
                                if (a.TryGetProperty("createdDate", out var cd) || a.TryGetProperty("CreatedDate", out cd))
                                {
                                    if (cd.TryGetDateTime(out var dt)) return dt;
                                    if (DateTime.TryParse(cd.GetString(), out var parsed)) return parsed;
                                }
                                return DateTime.MinValue;
                            })
                            .Take(10)
                            .ToList();

                        var top10Json = JsonSerializer.Serialize(top10, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        _memoryCache.Set("CachedTop10News", top10Json, TimeSpan.FromHours(7));
                        _memoryCache.Set("CachedNews", top10Json, TimeSpan.FromHours(7));
                        SaveSnapshotFile("top10_news.json", top10Json);
                        SaveSnapshotFile("news.json", top10Json);
                        _logger.LogInformation("Successfully cached TOP 10 newest articles with full content.");
                    }
                    else
                    {
                        _memoryCache.Set("CachedNews", newsJson, TimeSpan.FromHours(7));
                        SaveSnapshotFile("news.json", newsJson);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to extract top 10 articles: {Message}. Saving raw news response.", ex.Message);
                    _memoryCache.Set("CachedNews", newsJson, TimeSpan.FromHours(7));
                    SaveSnapshotFile("news.json", newsJson);
                }
            }

            // 3. Fetch & Cache Tags
            var tagRes = await coreClient.GetAsync("tag", cancellationToken);
            if (tagRes.IsSuccessStatusCode)
            {
                var tagJson = await tagRes.Content.ReadAsStringAsync(cancellationToken);
                _memoryCache.Set("CachedTags", tagJson, TimeSpan.FromHours(7));
                SaveSnapshotFile("tags.json", tagJson);
            }

            // 4. Fetch & Cache Dashboard Analytics
            var dashRes = await analyticsClient.GetAsync("analytics/dashboard", cancellationToken);
            if (dashRes.IsSuccessStatusCode)
            {
                var dashJson = await dashRes.Content.ReadAsStringAsync(cancellationToken);
                _memoryCache.Set("CachedDashboard", dashJson, TimeSpan.FromHours(7));
                SaveSnapshotFile("dashboard.json", dashJson);
            }

            _memoryCache.Set("CacheLastRefreshedAt", DateTime.UtcNow);
            _logger.LogInformation("Successfully refreshed in-memory cache and local snapshot files.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Background cache refresh encountered an API connection error: {Message}", ex.Message);
        }
    }

    private static void SaveSnapshotFile(string filename, string content)
    {
        try
        {
            var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cached_snapshots");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            var path = Path.Combine(dataDir, filename);
            File.WriteAllText(path, content);
        }
        catch
        {
            // Best effort write
        }
    }
}
