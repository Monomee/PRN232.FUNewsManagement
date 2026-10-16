using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.DataAccess.DAO;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Repositories;

public class SystemAccountRepository : ISystemAccountRepository
{
    public Task<List<SystemAccount>> GetAllAccountsAsync() => SystemAccountDAO.Instance.GetAllAsync();

    public Task<SystemAccount?> GetAccountByIdAsync(short id) => SystemAccountDAO.Instance.GetByIdAsync(id);

    public Task<SystemAccount?> GetAccountByEmailAsync(string email) => SystemAccountDAO.Instance.GetByEmailAsync(email);

    public Task<bool> IsEmailTakenAsync(string email, short? excludeAccountId = null) => 
        SystemAccountDAO.Instance.IsEmailTakenAsync(email, excludeAccountId);

    public Task<bool> HasCreatedNewsArticlesAsync(short accountId) => 
        SystemAccountDAO.Instance.HasCreatedNewsArticlesAsync(accountId);

    public Task<SystemAccount> AddAccountAsync(SystemAccount account) => SystemAccountDAO.Instance.AddAsync(account);

    public Task<SystemAccount?> UpdateAccountAsync(SystemAccount account) => SystemAccountDAO.Instance.UpdateAsync(account);

    public Task<bool> DeleteAccountAsync(short id) => SystemAccountDAO.Instance.DeleteAsync(id);
}
