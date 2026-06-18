using TimeOn.Maui.Features.Authentication.Views;
using TimeOn.Maui.Features.Customers.Views;
using TimeOn.Maui.Features.Dashboard.Views;
using TimeOn.Maui.Features.Settings.Views;
using TimeOn.Maui.Features.Tracking.Views;
using TimeOn.Maui.Features.Trips.Views;

namespace TimeOn.Maui.Extensions;

public static class RoutingExtensions
{
    public static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(TripsPage), typeof(TripsPage));
        Routing.RegisterRoute(nameof(TripDetailPage), typeof(TripDetailPage));
        Routing.RegisterRoute(nameof(CustomersPage), typeof(CustomersPage));
        Routing.RegisterRoute(nameof(CustomerFormPage), typeof(CustomerFormPage));
        Routing.RegisterRoute(nameof(TrackingPage), typeof(TrackingPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
