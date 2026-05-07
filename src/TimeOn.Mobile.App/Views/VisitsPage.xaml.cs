using TimeOn.Mobile.App.ViewModels;

namespace TimeOn.Mobile.App.Views;

public partial class VisitsPage : ContentPage
{
    public VisitsPage(VisitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
