using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.BusinessLogic.Services;

namespace HoangNV_SE1930_A01_BE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/accounts")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EnableQuery]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccount(short id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return NotFound(new { message = $"Account with ID {id} not found." });
        }
        return Ok(account);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _accountService.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetAccount), new { id = created.AccountId }, created);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating account: " + ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount(short id, [FromBody] UpdateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _accountService.UpdateAccountAsync(id, dto);
            return Ok(updated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating account: " + ex.Message });
        }
    }

    [HttpPut("{id}/password")]
    [HttpPatch("{id}/password")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ChangePassword(short id, [FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Staff can only update their own password; Admin can update any account's password
        if (User.IsInRole("Staff") && !User.IsInRole("Admin"))
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (short.TryParse(currentUserIdStr, out short currentUserId) && currentUserId != id)
            {
                return Forbid();
            }
        }

        try
        {
            await _accountService.ChangePasswordAsync(id, dto);
            return Ok(new { message = "Password updated successfully." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error changing password: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(short id)
    {
        try
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting account: " + ex.Message });
        }
    }
}
