using TimeOn.Domain.Entities;

namespace TimeOn.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<IReadOnlyList<Customer>> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);
}
