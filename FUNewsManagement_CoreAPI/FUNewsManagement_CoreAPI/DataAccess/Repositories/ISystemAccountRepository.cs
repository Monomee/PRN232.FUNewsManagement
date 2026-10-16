using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public interface ISystemAccountRepository
{
    Task<List<SystemAccount>> GetAllAccountsAsync();
    Task<SystemAccount?> GetAccountByIdAsync(short id);
    Task<SystemAccount?> GetAccountByEmailAsync(string email);
    Task<bool> IsEmailTakenAsync(string email, short? excludeAccountId = null);
    Task<bool> HasCreatedNewsArticlesAsync(short accountId);
    Task<SystemAccount> AddAccountAsync(SystemAccount account);
    Task<SystemAccount?> UpdateAccountAsync(SystemAccount account);
    Task<bool> DeleteAccountAsync(short id);
}
