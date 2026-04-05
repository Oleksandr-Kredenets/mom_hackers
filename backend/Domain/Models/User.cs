#pragma warning disable CS8618
namespace TMS.Domain.Models;

public readonly record struct RegisterRequest(string Name, string Email, string Password);
public readonly record struct LoginRequest(string Email, string Password);

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}
