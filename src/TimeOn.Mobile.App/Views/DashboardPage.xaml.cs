using TimeOn.Mobile.App.ViewModels;

namespace TimeOn.Mobile.App.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
