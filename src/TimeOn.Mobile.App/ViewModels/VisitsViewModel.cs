using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.App.ViewModels;

public partial class VisitsViewModel : BaseViewModel
{
    private readonly ICustomerVisitRepository customerVisitRepository;

    public ObservableCollection<CustomerVisit> Visits { get; } = [];

    public VisitsViewModel(ICustomerVisitRepository customerVisitRepository)
    {
        this.customerVisitRepository = customerVisitRepository;
        Title = "Customer Visits";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var visits = await customerVisitRepository.GetVisitsForDayAsync(DateOnly.FromDateTime(DateTime.Now));
            Visits.Clear();
            foreach (var visit in visits)
            {
                Visits.Add(visit);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
