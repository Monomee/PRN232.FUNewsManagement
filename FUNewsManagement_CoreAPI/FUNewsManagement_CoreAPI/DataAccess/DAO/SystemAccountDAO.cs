using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.DAO;

public class SystemAccountDAO : BaseDAO
{
    private static readonly object _lock = new object();
    private static SystemAccountDAO? _instance;

    private SystemAccountDAO() { }

    public static SystemAccountDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SystemAccountDAO();
                    }
                }
            }
            return _instance;
        }
    }

    public async Task<List<SystemAccount>> GetAllAsync()
    {
        using var context = CreateDbContext();
        return await context.SystemAccounts.AsNoTracking().ToListAsync();
    }

    public async Task<SystemAccount?> GetByIdAsync(short id)
    {
        using var context = CreateDbContext();
        return await context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountId == id);
    }

    public async Task<SystemAccount?> GetByEmailAsync(string email)
    {
        using var context = CreateDbContext();
        return await context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountEmail != null && a.AccountEmail.ToLower() == email.Trim().ToLower());
    }

    public async Task<bool> IsEmailTakenAsync(string email, short? excludeAccountId = null)
    {
        using var context = CreateDbContext();
        var normalizedEmail = email.Trim().ToLower();
        return await context.SystemAccounts.AnyAsync(a => a.AccountEmail != null && 
                                                         a.AccountEmail.ToLower() == normalizedEmail && 
                                                         (!excludeAccountId.HasValue || a.AccountId != excludeAccountId.Value));
    }

    public async Task<bool> HasCreatedNewsArticlesAsync(short accountId)
    {
        using var context = CreateDbContext();
        return await context.NewsArticles.AnyAsync(n => n.CreatedById == accountId);
    }

    public async Task<short> GetNextAccountIdAsync()
    {
        using var context = CreateDbContext();
        var maxId = await context.SystemAccounts.MaxAsync(a => (short?)a.AccountId);
        return (short)((maxId ?? 0) + 1);
    }

    public async Task<SystemAccount> AddAsync(SystemAccount account)
    {
        using var context = CreateDbContext();
        if (account.AccountId == 0)
        {
            account.AccountId = await GetNextAccountIdAsync();
        }
        await context.SystemAccounts.AddAsync(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<SystemAccount?> UpdateAsync(SystemAccount account)
    {
        using var context = CreateDbContext();
        var existing = await context.SystemAccounts.FirstOrDefaultAsync(a => a.AccountId == account.AccountId);
        if (existing == null) return null;

        existing.AccountName = account.AccountName;
        existing.AccountEmail = account.AccountEmail;
        if (!string.IsNullOrWhiteSpace(account.AccountPassword))
        {
            existing.AccountPassword = account.AccountPassword;
        }
        if (account.AccountRole.HasValue)
        {
            existing.AccountRole = account.AccountRole;
        }

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(short id)
    {
        using var context = CreateDbContext();
        var existing = await context.SystemAccounts.FirstOrDefaultAsync(a => a.AccountId == id);
        if (existing == null) return false;

        context.SystemAccounts.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
