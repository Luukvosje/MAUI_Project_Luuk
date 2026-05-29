using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence;

public abstract class TimeOnDbContextBase : DbContext
{
    protected TimeOnDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
    public DbSet<RideSegment> RideSegments => Set<RideSegment>();
    public DbSet<CustomerVisit> CustomerVisits => Set<CustomerVisit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<RideSegment>().Ignore(segment => segment.Checkpoints);
        base.OnModelCreating(modelBuilder);
    }
}
