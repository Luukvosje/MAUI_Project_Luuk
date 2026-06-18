using TimeOn.Maui.Features.Settings.ViewModels;

namespace TimeOn.Maui.Features.Settings.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
