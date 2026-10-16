using System;

namespace HoangNV_SE1930_A01_BE.DataAccess.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiryDate { get; set; }
    public short AccountId { get; set; }

    public virtual SystemAccount Account { get; set; } = null!;
}
