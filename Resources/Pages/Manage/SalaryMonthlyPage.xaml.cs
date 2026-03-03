namespace MyApp1;

public partial class SalaryMonthlyPage : ContentPage
{
    private SalaryMonthlyForm _vm;

    public SalaryMonthlyPage()
    {
        InitializeComponent();
        _vm = new SalaryMonthlyForm();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataAsync();
    }
}