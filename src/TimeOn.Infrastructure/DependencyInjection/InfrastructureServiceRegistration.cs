using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Abstractions;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Domain.Services;
using TimeOn.Infrastructure.Authentication;
using TimeOn.Infrastructure.External;
using TimeOn.Infrastructure.Configurations;
using TimeOn.Infrastructure.Persistence;
using TimeOn.Infrastructure.Repositories;
using TimeOn.Application.Interfaces;

namespace TimeOn.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<TimeOnApiSettings>(configuration.GetSection(TimeOnApiSettings.SectionName));
        services.Configure<LocalDatabaseSettings>(configuration.GetSection(LocalDatabaseSettings.SectionName));
        services.Configure<GoogleApiSettings>(configuration.GetSection(GoogleApiSettings.SectionName));

        var remoteConnectionString = ResolveRemoteConnectionString(configuration);
        var localConnectionString = ResolveLocalConnectionString(configuration);
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(remoteConnectionString));
        services.AddDbContext<LocalDbContext>(options => options.UseSqlite(localConnectionString));

        services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        services.AddScoped<ILocalWorkSessionRepository, WorkSessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGeoLocationService, GoogleGeocodingService>();
        services.AddSingleton<IDistanceCalculator, DistanceCalculator>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }

    private static string ResolveLocalConnectionString(IConfiguration configuration)
    {
        var configured = configuration["LocalDatabase:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var builder = new SqliteConnectionStringBuilder(configured);
            if (!string.IsNullOrWhiteSpace(builder.DataSource) && !Path.IsPathRooted(builder.DataSource))
            {
                builder.DataSource = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    builder.DataSource);
            }

            return builder.ToString();
        }

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "timeon-local.db");

        return new SqliteConnectionStringBuilder
        {
            DataSource = dbPath
        }.ToString();
    }

    private static string ResolveRemoteConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:RemoteConnectionString"]
            ?? throw new InvalidOperationException("Remote SQL Server connection string is not configured.");

        return connectionString;
    }
}
