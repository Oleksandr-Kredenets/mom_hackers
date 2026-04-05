namespace TMS.Application.DTOs;

public record AuthUserDto(Guid Id, string Name, string Email);
public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, AuthUserDto User);
