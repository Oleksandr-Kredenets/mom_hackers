namespace TMS.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(Guid userId, string email, string name);
}
