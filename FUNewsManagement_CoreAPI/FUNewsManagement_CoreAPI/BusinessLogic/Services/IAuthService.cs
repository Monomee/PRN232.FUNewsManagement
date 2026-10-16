using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<SystemAccountDto?> GetCurrentUserAsync(short accountId, string role);
}
