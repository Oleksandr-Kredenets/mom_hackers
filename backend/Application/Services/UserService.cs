using TMS.Application.Interfaces;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using Microsoft.AspNetCore.Identity;
using TMS.Application.DTOs;

namespace TMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

   public Task<User> RegisterAsync(string name, string email, string password)
   {
        var account = new User()
        {
            Name = name,
            Email = email,
        };
        var passwordHash = _passwordHasher.HashPassword(account, password);
        account.PasswordHash = passwordHash;
        return _userRepository.CreateUserAsync(account);
   }

   public async Task<AuthResponse> LoginAsync(string email, string password)
   {
    var account = await _userRepository.GetUserByEmailAsync(email);
    var passwordHash = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);
    if (passwordHash == PasswordVerificationResult.Failed)
        throw new InvalidOperationException("Invalid password.");

    var (token, expiresAt) = _jwtTokenService.CreateToken(account.Id, account.Email, account.Name);
    return new AuthResponse(token, expiresAt, new AuthUserDto(account.Id, account.Name, account.Email));
   }
}
