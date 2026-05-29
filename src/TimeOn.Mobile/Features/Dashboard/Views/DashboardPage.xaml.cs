using TimeOn.Mobile.Features.Dashboard.ViewModels;

namespace TimeOn.Mobile.Features.Dashboard.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
