namespace MyApp1;

public partial class DoorWarehouseContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    // —сылка на данные дверей внутри общего заказа
    public DoorForm DoorData => CurrentOrder?.DoorData;

    public DoorWarehouseContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;

            // ѕрив€зываем контекст напр€мую к DoorData (как было в оригинале)
            BindingContext = DoorData;
        }
    }

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
}