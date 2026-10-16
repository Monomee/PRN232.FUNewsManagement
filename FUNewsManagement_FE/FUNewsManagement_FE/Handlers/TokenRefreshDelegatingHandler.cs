using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FUNewsManagement_FE.Handlers;

public class TokenRefreshDelegatingHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenRefreshDelegatingHandler> _logger;

    public TokenRefreshDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<TokenRefreshDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var token = GetCurrentAccessToken(httpContext);

        if (!string.IsNullOrEmpty(token) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If received 401 Unauthorized, initiate thread-safe token refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && httpContext != null)
        {
            _logger.LogInformation("Received 401 Unauthorized from backend API. Attempting thread-safe token refresh...");

            bool acquired = await _refreshLock.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            if (acquired)
            {
                try
                {
                    // Check if token was already refreshed by another concurrent thread
                    var latestToken = GetCurrentAccessToken(httpContext);
                    if (!string.IsNullOrEmpty(latestToken) && latestToken != token)
                    {
                        _logger.LogInformation("Token was already refreshed by another thread. Replaying request with new token.");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", latestToken);
                        return await base.SendAsync(request, cancellationToken);
                    }

                    var refreshToken = httpContext.Session.GetString("refresh_token") 
                                       ?? httpContext.User.FindFirstValue("RefreshToken");

                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
                    {
                        var coreApiUrl = _configuration["ApiSettings:CoreApiUrl"] ?? "http://localhost:5132/api/";
                        using var refreshClient = new HttpClient();
                        var refreshUrl = new Uri(new Uri(coreApiUrl), "auth/refresh");

                        var refreshPayload = new
                        {
                            token = token,
                            refreshToken = refreshToken
                        };

                        var refreshResponse = await refreshClient.PostAsJsonAsync(refreshUrl, refreshPayload, cancellationToken);
                        if (refreshResponse.IsSuccessStatusCode)
                        {
                            var refreshContent = await refreshResponse.Content.ReadAsStringAsync(cancellationToken);
                            using var doc = JsonDocument.Parse(refreshContent);
                            var root = doc.RootElement;

                            var newAccessToken = root.GetProperty("token").GetString();
                            var newRefreshToken = root.GetProperty("refreshToken").GetString();

                            if (!string.IsNullOrEmpty(newAccessToken))
                            {
                                httpContext.Session.SetString("access_token", newAccessToken);
                                if (!string.IsNullOrEmpty(newRefreshToken))
                                {
                                    httpContext.Session.SetString("refresh_token", newRefreshToken);
                                }

                                _logger.LogInformation("Token refreshed successfully. Replaying original HTTP request.");
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                                return await base.SendAsync(request, cancellationToken);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Token refresh failed with status code: {StatusCode}", refreshResponse.StatusCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token refresh.");
                }
                finally
                {
                    _refreshLock.Release();
                }
            }
        }

        return response;
    }

    private static string? GetCurrentAccessToken(HttpContext? httpContext)
    {
        if (httpContext == null) return null;
        return httpContext.Session.GetString("access_token")
               ?? httpContext.User.FindFirstValue("JwtToken");
    }
}
