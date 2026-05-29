using Microsoft.EntityFrameworkCore;

namespace TimeOn.Infrastructure.Persistence;

public sealed class AppDbContext : TimeOnDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
