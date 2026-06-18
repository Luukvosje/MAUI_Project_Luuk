using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TimeOn.Application.Features.Auth.Services;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Application.Features.Dashboard.Services;
using TimeOn.Application.Features.Trips.Services;
using TimeOn.Application.Features.WorkSessions.Services;

namespace TimeOn.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IWorkSessionService, WorkSessionService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
