using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly ISystemAccountRepository _accountRepository;
    private readonly INewsArticleRepository _articleRepository;
    private readonly IConfiguration _configuration;

    public AccountService(
        ISystemAccountRepository accountRepository, 
        INewsArticleRepository articleRepository,
        IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        _articleRepository = articleRepository;
        _configuration = configuration;
    }

    public async Task<List<SystemAccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAccountsAsync();
        var articles = await _articleRepository.GetAllNewsArticlesAsync();

        return accounts.Select(a => new SystemAccountDto
        {
            AccountId = a.AccountId,
            AccountName = a.AccountName,
            AccountEmail = a.AccountEmail,
            AccountRole = a.AccountRole,
            CreatedArticlesCount = articles.Count(ar => ar.CreatedById == a.AccountId)
        }).ToList();
    }

    public async Task<SystemAccountDto?> GetAccountByIdAsync(short id)
    {
        var account = await _accountRepository.GetAccountByIdAsync(id);
        if (account == null) return null;

        var articles = await _articleRepository.GetAllNewsArticlesAsync();

        return new SystemAccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole,
            CreatedArticlesCount = articles.Count(ar => ar.CreatedById == account.AccountId)
        };
    }

    public async Task<SystemAccountDto> CreateAccountAsync(CreateAccountDto dto)
    {
        var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
        if (dto.AccountEmail.Trim().Equals(adminEmail.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Cannot use the administrator email address.");
        }

        if (await _accountRepository.IsEmailTakenAsync(dto.AccountEmail))
        {
            throw new BusinessRuleException($"Account email '{dto.AccountEmail}' is already registered.");
        }

        var account = new SystemAccount
        {
            AccountName = dto.AccountName.Trim(),
            AccountEmail = dto.AccountEmail.Trim(),
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword.Trim()
        };

        var created = await _accountRepository.AddAccountAsync(account);

        return new SystemAccountDto
        {
            AccountId = created.AccountId,
            AccountName = created.AccountName,
            AccountEmail = created.AccountEmail,
            AccountRole = created.AccountRole,
            CreatedArticlesCount = 0
        };
    }

    public async Task<SystemAccountDto> UpdateAccountAsync(short id, UpdateAccountDto dto)
    {
        var existing = await _accountRepository.GetAccountByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Account with ID {id} does not exist.");
        }

        var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
        if (dto.AccountEmail.Trim().Equals(adminEmail.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Cannot use the administrator email address.");
        }

        if (await _accountRepository.IsEmailTakenAsync(dto.AccountEmail, id))
        {
            throw new BusinessRuleException($"Account email '{dto.AccountEmail}' is already used by another account.");
        }

        existing.AccountName = dto.AccountName.Trim();
        existing.AccountEmail = dto.AccountEmail.Trim();
        existing.AccountRole = dto.AccountRole;
        if (!string.IsNullOrWhiteSpace(dto.AccountPassword))
        {
            existing.AccountPassword = dto.AccountPassword.Trim();
        }

        var updated = await _accountRepository.UpdateAccountAsync(existing);

        return new SystemAccountDto
        {
            AccountId = updated!.AccountId,
            AccountName = updated.AccountName,
            AccountEmail = updated.AccountEmail,
            AccountRole = updated.AccountRole
        };
    }

    public async Task ChangePasswordAsync(short id, ChangePasswordDto dto)
    {
        var existing = await _accountRepository.GetAccountByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Account with ID {id} does not exist.");
        }

        if (existing.AccountPassword != dto.CurrentPassword.Trim())
        {
            throw new BusinessRuleException("The current password is not correct.");
        }

        existing.AccountPassword = dto.NewPassword.Trim();
        await _accountRepository.UpdateAccountAsync(existing);
    }

    public async Task DeleteAccountAsync(short id)
    {
        if (id == 0)
        {
            throw new BusinessRuleException("Cannot delete the system Administrator account.");
        }

        var existing = await _accountRepository.GetAccountByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Account with ID {id} does not exist.");
        }

        // Check foreign key constraint: Cannot delete if account has created any record in NewsArticle.CreatedByID
        if (await _accountRepository.HasCreatedNewsArticlesAsync(id))
        {
            throw new BusinessRuleException($"Cannot delete account '{existing.AccountName}' (ID: {id}) because this account has created existing news articles.");
        }

        await _accountRepository.DeleteAccountAsync(id);
    }
}
