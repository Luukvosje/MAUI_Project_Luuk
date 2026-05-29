using TimeOn.Domain.Entities;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.RepositoryInterfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userGuid);
    Task<User?> GetByEmailAsync(Email email);
    Task<bool> ExistsByEmailAsync(Email email);
    Task AddAsync(User user);
    void Update(User user);
}
