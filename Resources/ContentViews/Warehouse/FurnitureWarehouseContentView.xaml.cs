namespace MyApp1;

public partial class FurnitureWarehouseContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    // —сылка на фурнитуру из общего заказа
    public FurnitureForm Furniture => CurrentOrder?.Furniture;

    public FurnitureWarehouseContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;
            // BindingContext остаЄтс€ this, как в оригинале
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

    // ≈сли позже понадобитс€ OnFinishClicked Ч можно добавить, но пока убрано
    // private async void OnFinishClicked(object sender, EventArgs e) { ... }
}