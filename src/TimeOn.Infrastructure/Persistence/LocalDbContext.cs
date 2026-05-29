using Microsoft.EntityFrameworkCore;

namespace TimeOn.Infrastructure.Persistence;

public sealed class LocalDbContext : TimeOnDbContextBase
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options)
        : base(options)
    {
    }
}
