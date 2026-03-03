namespace MyApp1;

public partial class SoftlyWarehouseContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    // Ссылка на данные мягкой зоны внутри общего заказа
    public SoftlyForm SoftlyData => CurrentOrder?.SoftlyData;

    public SoftlyWarehouseContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;

            // Привязываем контекст напрямую к SoftlyData (как в оригинале)
            BindingContext = SoftlyData;
        }
    }

    // Авто-выделение текста при нажатии на поле
    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    // Если позже понадобится OnNavigateToFurnitureClicked — можно добавить, но пока убрано
    // private async void OnNavigateToFurnitureClicked(object sender, EventArgs e) { ... }
}