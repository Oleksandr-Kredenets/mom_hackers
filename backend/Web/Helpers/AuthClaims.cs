using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TMS.Infrastructure.Security;

namespace Web.Helpers;

public static class AuthClaims
{
    public static string? FirstClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirstValue(claimType);
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return null;
    }

    public static bool TryGetAuthUser(ClaimsPrincipal principal, out AuthUserDto? user)
    {
        user = null;
        var idText = FirstClaimValue(
            principal,
            ClaimTypes.NameIdentifier,
            JwtRegisteredClaimNames.Sub,
            "sub");
        if (!Guid.TryParse(idText, out var id))
            return false;

        var email = FirstClaimValue(
            principal,
            ClaimTypes.Email,
            JwtRegisteredClaimNames.Email,
            "email") ?? "";

        var name = FirstClaimValue(
            principal,
            ClaimTypes.Name,
            JwtRegisteredClaimNames.UniqueName,
            "unique_name",
            "name") ?? "";

        user = new AuthUserDto(id, name, email);
        return true;
    }
}
