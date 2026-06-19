using FluentAssertions;
using NSubstitute;
using TimeOn.Maui.Features.Authentication.ViewModels;
using TimeOn.Maui.Features.Authentication.Views;
using TimeOn.Maui.Interfaces;
using TimeOn.UITests.Infrastructure;

namespace TimeOn.UITests.Authentication;

[Collection(nameof(MauiUiTestCollection))]
public class LoginPageTests
{
    [Fact]
    public async Task LoginPage_ShowsValidationErrorInUi_WhenCredentialsAreRejected()
    {
        const string expectedError = "Invalid email or password.";

        var authenticationService = Substitute.For<IAuthenticationService>();
        authenticationService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);
        authenticationService.ErrorMessage.Returns(expectedError);

        var viewModel = new LoginViewModel(authenticationService)
        {
            Email = "user@example.com",
            Password = "wrong-password"
        };

        var page = new LoginPage(viewModel);

        await viewModel.LoginCommand.ExecuteAsync(null);

        var errorLabel = VisualTreeHelper.FindLabelByText(page, expectedError);
        errorLabel.Should().NotBeNull();
        errorLabel!.IsVisible.Should().BeTrue();
        viewModel.IsBusy.Should().BeFalse();
    }
}
