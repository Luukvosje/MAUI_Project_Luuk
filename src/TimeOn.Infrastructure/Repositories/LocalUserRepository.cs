using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Domain.ValueObjects;
using TimeOn.Infrastructure.Persistence;

namespace TimeOn.Infrastructure.Repositories;

public sealed class LocalUserRepository : IUserRepository
{
    private readonly LocalDbContext _context;

    public LocalUserRepository(LocalDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userGuid)
    {
        return await _context.Users.FindAsync(userGuid);
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(user => user.Email.Value == email.Value);
    }

    public async Task<bool> ExistsByEmailAsync(Email email)
    {
        return await _context.Users
            .AnyAsync(user => user.Email.Value == email.Value);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }
}
