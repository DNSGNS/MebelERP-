namespace MyApp1;

public partial class CuttingEdgeContentView : ContentView
{
    private CuttingData ViewModel => BindingContext as CuttingData;

    public CuttingEdgeContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (ViewModel != null)
        {
            // Подписываемся на изменения каждой детали, чтобы обновлять "Метры" в реальном времени
            foreach (var detail in ViewModel.DetailsForm.Details)
            {
                detail.PropertyChanged += (s, e) => ViewModel.RefreshTotals();
            }
        }
    }
}