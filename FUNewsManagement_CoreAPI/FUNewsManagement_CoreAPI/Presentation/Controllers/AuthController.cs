using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during authentication: " + ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during token refresh: " + ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(idClaim) || string.IsNullOrEmpty(roleClaim))
        {
            return Unauthorized();
        }

        short.TryParse(idClaim, out short accountId);
        var user = await _authService.GetCurrentUserAsync(accountId, roleClaim);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }
}
