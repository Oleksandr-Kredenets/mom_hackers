using TMS.Application.DTOs;
using TMS.Domain.Models;

namespace TMS.Application.Interfaces;

public interface IUserService
{
    Task<User> RegisterAsync(string name, string email, string password);
    Task<AuthResponse> LoginAsync(string email, string password);
}
