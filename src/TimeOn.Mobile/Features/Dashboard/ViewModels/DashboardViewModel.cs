using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeOn.Mobile.Features.Dashboard.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string WelcomeMessage { get; set; } = "TimeOn Mobile";
}
