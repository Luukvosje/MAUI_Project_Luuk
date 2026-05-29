using TimeOn.Mobile.Features.Authentication.Views;
using TimeOn.Mobile.Features.Customers.Views;
using TimeOn.Mobile.Features.Dashboard.Views;
using TimeOn.Mobile.Features.Settings.Views;
using TimeOn.Mobile.Features.Tracking.Views;
using TimeOn.Mobile.Features.Trips.Views;

namespace TimeOn.Mobile.Extensions;

public static class RoutingExtensions
{
    public static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(TripsPage), typeof(TripsPage));
        Routing.RegisterRoute(nameof(CustomersPage), typeof(CustomersPage));
        Routing.RegisterRoute(nameof(CustomerFormPage), typeof(CustomerFormPage));
        Routing.RegisterRoute(nameof(TrackingPage), typeof(TrackingPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
