using System.Collections.Generic;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface IAccountService
{
    Task<List<SystemAccountDto>> GetAllAccountsAsync();
    Task<SystemAccountDto?> GetAccountByIdAsync(short id);
    Task<SystemAccountDto> CreateAccountAsync(CreateAccountDto dto);
    Task<SystemAccountDto> UpdateAccountAsync(short id, UpdateAccountDto dto);
    Task ChangePasswordAsync(short id, ChangePasswordDto dto);
    Task DeleteAccountAsync(short id);
}
