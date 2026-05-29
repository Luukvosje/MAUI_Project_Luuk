using Microsoft.Extensions.Logging;

namespace TimeOn.Infrastructure.Persistence;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly LocalDbContext _localDbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        LocalDbContext localDbContext,
        ILogger<DatabaseInitializer> logger)
    {
        _localDbContext = localDbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _localDbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Local SQLite database initialization failed.");
        }
    }
}
