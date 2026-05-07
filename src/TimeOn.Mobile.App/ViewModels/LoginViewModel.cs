using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Core.Interfaces;

namespace TimeOn.Mobile.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService authService;

    [ObservableProperty]
    private string email = "student@timeon.local";

    [ObservableProperty]
    private string password = "Password123!";

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        this.authService = authService;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        try
        {
            bool success = await authService.LoginAsync(Email, Password);
            StatusMessage = success ? "Login successful." : "Login failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
