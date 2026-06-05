using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Abstractions;
using TimeOn.Domain.Interfaces;
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
        services.Configure<GoogleApiSettings>(configuration.GetSection(GoogleApiSettings.SectionName));

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGeoLocationService, GoogleGeocodingService>();
        services.AddSingleton<IDistanceCalculator, DistanceCalculator>();

        return services;
    }
}
