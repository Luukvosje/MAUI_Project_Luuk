using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeOn.Mobile.App.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;
}
