using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return RedirectByRole(role);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("CoreApiClient");
            var response = await client.PostAsJsonAsync("auth/login", new
            {
                Email = Email,
                Password = Password
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(errorJson);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                    {
                        ErrorMessage = msg.GetString();
                    }
                    else
                    {
                        ErrorMessage = "Invalid email or password.";
                    }
                }
                catch
                {
                    ErrorMessage = "Authentication failed. Please check your credentials.";
                }
                return Page();
            }

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResultDto>();
            if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
            {
                ErrorMessage = "Received invalid response from server.";
                return Page();
            }

            // Create Claims for Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.AccountId.ToString()),
                new Claim(ClaimTypes.Name, loginResult.AccountName ?? "User"),
                new Claim(ClaimTypes.Email, loginResult.Email),
                new Claim(ClaimTypes.Role, loginResult.Role),
                new Claim("AccountRole", (loginResult.AccountRole ?? 0).ToString()),
                new Claim("JwtToken", loginResult.Token),
                new Claim("RefreshToken", loginResult.RefreshToken ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Store in Session
            HttpContext.Session.SetString("JwtToken", loginResult.Token);
            HttpContext.Session.SetString("access_token", loginResult.Token);
            HttpContext.Session.SetString("refresh_token", loginResult.RefreshToken ?? "");
            HttpContext.Session.SetString("UserRole", loginResult.Role);
            HttpContext.Session.SetString("UserName", loginResult.AccountName ?? "User");
            HttpContext.Session.SetString("UserId", loginResult.AccountId.ToString());

            // Set client-accessible cookies for Javascript Fetch/SignalR
            Response.Cookies.Append("jwt_token", loginResult.Token, new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            if (!string.IsNullOrEmpty(loginResult.RefreshToken))
            {
                Response.Cookies.Append("refresh_token", loginResult.RefreshToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return RedirectByRole(loginResult.Role);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not connect to Core API service: " + ex.Message;
            return Page();
        }
    }

    private IActionResult RedirectByRole(string? role)
    {
        return role switch
        {
            "Admin" => RedirectToPage("/Admin/Reports"),
            "Staff" => RedirectToPage("/Staff/Articles"),
            "Lecturer" => RedirectToPage("/Lecturer/Articles"),
            _ => RedirectToPage("/Index")
        };
    }
}

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? AccountRole { get; set; }
}
