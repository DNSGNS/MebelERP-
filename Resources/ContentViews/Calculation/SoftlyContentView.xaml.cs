namespace MyApp1;

public partial class SoftlyContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    // Ссылка на данные мягкой зоны внутри общего заказа
    public SoftlyForm SoftlyData => CurrentOrder?.SoftlyData;

    public SoftlyContentView()
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

    private void ToWidth(object sender, EventArgs e) => EntWidth.Focus();
    private void ToCount(object sender, EventArgs e) => EntCount.Focus();

    private void ToApron(object sender, EventArgs e) => EntApron.Focus();
    private void ToAdditional(object sender, EventArgs e) => EntAdditional.Focus();
}