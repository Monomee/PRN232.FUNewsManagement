using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;
using FUNewsManagement_FE.BackgroundServices;
using FUNewsManagement_FE.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Staff", "StaffOnly");
    options.Conventions.AuthorizeFolder("/Lecturer", "LecturerOnly");
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenRefreshDelegatingHandler>();

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
    options.AddPolicy("LecturerOnly", policy => policy.RequireRole("Lecturer"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("InternalUsers", policy => policy.RequireRole("Staff", "Lecturer", "Admin"));
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// API URLs from configuration
var coreApiUrl = builder.Configuration["ApiSettings:CoreApiUrl"] ?? "http://localhost:5132/api/";
var analyticsApiUrl = builder.Configuration["ApiSettings:AnalyticsApiUrl"] ?? "http://localhost:5133/api/";
var aiApiUrl = builder.Configuration["ApiSettings:AiApiUrl"] ?? "http://localhost:5134/api/";

// Polly WaitAndRetry policy with Jitter & visual logging
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 300)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var reason = outcome.Exception?.Message ?? (outcome.Result != null ? $"HTTP {(int)outcome.Result.StatusCode} {outcome.Result.StatusCode}" : "Lỗi mạng");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Polly Retry Policy] Phát hiện sự cố ({reason}). Đang chờ {timespan.TotalSeconds:F2}s trước khi Retry #{retryAttempt} of 3...");
                Console.ResetColor();
            });
}

// 1. Core API Client (Includes Token Refresh handler & Polly Retry)
builder.Services.AddHttpClient("CoreApiClient", client =>
{
    client.BaseAddress = new Uri(coreApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy());

// Keep legacy "BackendApi" pointing to CoreApiClient for backward compatibility
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(coreApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy());

// 2. Analytics API Client
builder.Services.AddHttpClient("AnalyticsApiClient", client =>
{
    client.BaseAddress = new Uri(analyticsApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy());

// 3. AI API Client
builder.Services.AddHttpClient("AiApiClient", client =>
{
    client.BaseAddress = new Uri(aiApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(GetRetryPolicy());

// Background Worker: 6-hour cache refresh
builder.Services.AddHostedService<DataCacheRefreshWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
