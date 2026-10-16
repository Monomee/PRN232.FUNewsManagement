using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Context;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ISystemAccountRepository _accountRepository;
    private readonly FunewsManagementContext _context;

    public AuthService(
        IConfiguration configuration, 
        ISystemAccountRepository accountRepository,
        FunewsManagementContext context)
    {
        _configuration = configuration;
        _accountRepository = accountRepository;
        _context = context;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var inputEmail = request.Email.Trim().ToLower();
        var inputPassword = request.Password.Trim();

        // 1. Check Admin from appsettings.json
        var adminEmail = (_configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org").Trim().ToLower();
        var adminPassword = (_configuration["AdminAccount:Password"] ?? "@@abc123@@").Trim();
        var adminRole = _configuration["AdminAccount:Role"] ?? "Admin";

        if (inputEmail == adminEmail && inputPassword == adminPassword)
        {
            var (token, jti, expiresAt) = GenerateJwtToken(0, "Administrator", adminEmail, adminRole, 0);
            var refreshToken = GenerateRefreshTokenString();

            // Save refresh token
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                IsUsed = false,
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                AccountId = 0
            });
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                AccountId = 0,
                AccountName = "Administrator",
                Email = adminEmail,
                Role = adminRole,
                AccountRole = 0
            };
        }

        // 2. Check SystemAccount in Database
        var account = await _accountRepository.GetAccountByEmailAsync(inputEmail);
        if (account == null || account.AccountPassword != inputPassword)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        string roleName = account.AccountRole switch
        {
            1 => "Staff",
            2 => "Lecturer",
            _ => "Guest"
        };

        var (jwtToken, tokenJti, tokenExpiresAt) = GenerateJwtToken(
            account.AccountId, 
            account.AccountName ?? "User", 
            account.AccountEmail ?? inputEmail, 
            roleName, 
            account.AccountRole ?? 0);

        var rfToken = GenerateRefreshTokenString();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = rfToken,
            JwtId = tokenJti,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AccountId = account.AccountId
        });
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = rfToken,
            ExpiresAt = tokenExpiresAt,
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "User",
            Email = account.AccountEmail ?? inputEmail,
            Role = roleName,
            AccountRole = account.AccountRole
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            throw new BusinessRuleException("Invalid access token.");
        }

        var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var accountIdStr = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(jti) || !short.TryParse(accountIdStr, out short accountId))
        {
            throw new BusinessRuleException("Malformed token claims.");
        }

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

        if (storedToken == null)
        {
            throw new BusinessRuleException("Refresh token does not exist.");
        }

        // Token Reuse Detection
        if (storedToken.IsUsed || storedToken.IsRevoked)
        {
            // Revoke all active refresh tokens for this account
            var compromisedTokens = await _context.RefreshTokens
                .Where(r => r.AccountId == storedToken.AccountId && !r.IsRevoked)
                .ToListAsync();

            foreach (var t in compromisedTokens)
            {
                t.IsRevoked = true;
            }
            await _context.SaveChangesAsync();

            throw new BusinessRuleException("Refresh token was already used or revoked! Security alert: all sessions for this account have been invalidated.");
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            throw new BusinessRuleException("Refresh token has expired. Please log in again.");
        }

        if (storedToken.JwtId != jti)
        {
            throw new BusinessRuleException("Access token does not match the provided refresh token.");
        }

        // Mark old token as used
        storedToken.IsUsed = true;

        // Retrieve user info
        string roleName = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guest";
        string accountName = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "User";
        string email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
        int accountRole = int.TryParse(principal.Claims.FirstOrDefault(c => c.Type == "AccountRole")?.Value, out var ar) ? ar : 0;

        // Issue new JWT & new Refresh Token
        var (newJwt, newJti, newExpiresAt) = GenerateJwtToken(accountId, accountName, email, roleName, accountRole);
        var newRefreshToken = GenerateRefreshTokenString();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            JwtId = newJti,
            IsUsed = false,
            IsRevoked = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AccountId = accountId
        });

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = newJwt,
            RefreshToken = newRefreshToken,
            ExpiresAt = newExpiresAt,
            AccountId = accountId,
            AccountName = accountName,
            Email = email,
            Role = roleName,
            AccountRole = accountRole
        };
    }

    public async Task<SystemAccountDto?> GetCurrentUserAsync(short accountId, string role)
    {
        if (role == "Admin" || accountId == 0)
        {
            return new SystemAccountDto
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org",
                AccountRole = 0
            };
        }

        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null) return null;

        return new SystemAccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };
    }

    private (string token, string jti, DateTime expiresAt) GenerateJwtToken(
        short accountId, string accountName, string email, string role, int accountRole)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "FUNewsManagementSystem_SuperSecretKey_2024_SecurityToken_@123456";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FUNewsManagementSystem_BE";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "FUNewsManagementSystem_FE";

        // Short-lived JWT (15 mins)
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var jti = Guid.NewGuid().ToString();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, accountName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("AccountRole", accountRole.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jti, expiresAt);
    }

    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "FUNewsManagementSystem_SuperSecretKey_2024_SecurityToken_@123456";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FUNewsManagementSystem_BE";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "FUNewsManagementSystem_FE";

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = false // Ignore lifetime here to extract expired claims
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
