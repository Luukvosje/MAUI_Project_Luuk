using System.Windows.Input;

namespace TimeOn.Maui.Features.Customers.Views;

public partial class CustomerFormView : ContentView
{
    public static readonly BindableProperty SaveCommandProperty =
        BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(CustomerFormView));

    public static readonly BindableProperty CancelCommandProperty =
        BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(CustomerFormView));

    public CustomerFormView()
    {
        InitializeComponent();
    }

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is not ViewModels.CustomerFormViewModel viewModel)
        {
            return;
        }

        SaveCommand ??= viewModel.SaveCommand;
        CancelCommand ??= viewModel.CancelCommand;
    }
}
