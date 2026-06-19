using FluentAssertions;
using NSubstitute;
using TimeOn.Maui.Features.Tracking.ViewModels;
using TimeOn.Maui.Features.Tracking.Views;
using TimeOn.Maui.Interfaces;
using TimeOn.UITests.Infrastructure;

namespace TimeOn.UITests.Tracking;

[Collection(nameof(MauiUiTestCollection))]
public class TrackingPageTests
{
    [Fact]
    public void TrackingPage_HidesDevelopmentTools_WhenDevelopmentModeIsDisabled()
    {
        var gpsTrackingService = Substitute.For<IGpsTrackingService>();
        var developmentModeService = Substitute.For<IDevelopmentModeService>();
        developmentModeService.IsSupported.Returns(true);
        developmentModeService.IsEnabled.Returns(false);

        var viewModel = new TrackingViewModel(gpsTrackingService, developmentModeService);
        var page = new TrackingPage(viewModel);

        viewModel.OnAppearing();

        viewModel.ShowDevelopmentTools.Should().BeFalse();

        var visibleLabels = VisualTreeHelper.VisibleLabels(page).Select(label => label.Text).ToList();
        visibleLabels.Should().NotContain("Development");
        visibleLabels.Should().NotContain("Save work session");
    }
}
