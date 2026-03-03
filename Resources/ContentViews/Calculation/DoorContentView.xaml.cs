namespace MyApp1;

public partial class DoorContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }
    public DoorForm DoorData => CurrentOrder?.DoorData;

    public DoorContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;
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

    // Методы перехода фокуса
    private void ToRouterLength(object sender, EventArgs e) => EntRouterLength.Focus();
    private void ToRouterWidth(object sender, EventArgs e) => EntRouterWidth.Focus();
    private void ToRouterCount(object sender, EventArgs e) => EntRouterCount.Focus();

    private void ToMirrorLength(object sender, EventArgs e) => EntMirrorLength.Focus();
    private void ToMirrorWidth(object sender, EventArgs e) => EntMirrorWidth.Focus();
    private void ToMirrorCount(object sender, EventArgs e) => EntMirrorCount.Focus();

    private void ToLdspLength(object sender, EventArgs e) => EntLdspLength.Focus();
    private void ToLdspWidth(object sender, EventArgs e) => EntLdspWidth.Focus();
    private void ToLdspCount(object sender, EventArgs e) => EntLdspCount.Focus();

    private void ToOtherLength(object sender, EventArgs e) => EntOtherLength.Focus();
    private void ToOtherWidth(object sender, EventArgs e) => EntOtherWidth.Focus();
    private void ToOtherCount(object sender, EventArgs e) => EntOtherCount.Focus();
}