using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
    public DbSet<DrivingSegment> DrivingSegments => Set<DrivingSegment>();
    public DbSet<StationarySegment> StationarySegments => Set<StationarySegment>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
