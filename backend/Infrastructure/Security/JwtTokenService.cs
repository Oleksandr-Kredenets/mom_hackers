using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TMS.Application.Interfaces;

namespace TMS.Infrastructure.Security;

public sealed class JwtTokenService : IJwtTokenService
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(24);

    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(string signingKey)
    {
        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException(
                "AUTH_SECRET must be at least 32 UTF-8 bytes for HS256 signing.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(Guid userId, string email, string name)
    {
        var expiresAt = DateTime.UtcNow.Add(DefaultLifetime);
        var subject = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, name),
        });

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = expiresAt,
            SigningCredentials = _signingCredentials,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return (handler.WriteToken(token), expiresAt);
    }
}
