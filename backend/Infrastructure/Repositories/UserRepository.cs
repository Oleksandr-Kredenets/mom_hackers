using Microsoft.EntityFrameworkCore;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure;
//using Microsoft.AspNetCore.Identity;

namespace TMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TmsDbContext _context;

    public UserRepository(TmsDbContext context)
    {
        _context = context;
    }
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException($"User {id} not found.");
        return user;
    }
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidOperationException($"User {email} not found.");
        return user;
    }
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<User> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException($"User {id} not found.");
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
