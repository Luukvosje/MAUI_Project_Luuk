using FluentAssertions;
using NSubstitute;
using TimeOn.Maui.Features.Dashboard.Services;
using TimeOn.Maui.Features.Dashboard.ViewModels;
using TimeOn.Maui.Features.Dashboard.Views;
using TimeOn.Maui.Features.Tracking.Models;
using TimeOn.Maui.Interfaces;
using TimeOn.UITests.Infrastructure;

namespace TimeOn.UITests.Dashboard;

[Collection(nameof(MauiUiTestCollection))]
public class DashboardPageTests
{
    [Fact]
    public void DashboardPage_ShowsEmptyState_WhenUserHasNoRecordedTrips()
    {
        var dashboardSummaryService = Substitute.For<IDashboardSummaryService>();
        var gpsTrackingService = Substitute.For<IGpsTrackingService>();
        gpsTrackingService.State.Returns(TrackingState.Idle);

        var viewModel = new DashboardViewModel(dashboardSummaryService, gpsTrackingService)
        {
            IsLoading = false,
            HasAnyActivity = false,
            ErrorMessage = null
        };

        var page = new DashboardPage(viewModel);

        viewModel.ShowEmptyState.Should().BeTrue();
        viewModel.ShowDayOverview.Should().BeFalse();

        var visibleLabels = VisualTreeHelper.VisibleLabels(page).Select(label => label.Text).ToList();
        visibleLabels.Should().Contain("No trips recorded yet");
        visibleLabels.Should().NotContain("Driving distance");
    }
}
