namespace MyApp1;

public partial class FurnitureContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    // —сылка на фурнитуру из общего заказа
    public FurnitureForm Furniture => CurrentOrder?.Furniture;

    public FurnitureContentView()
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

    private void ToNaprBez(object sender, EventArgs e) => EntNaprBez.Focus();
    private void ToNaprS(object sender, EventArgs e) => EntNaprS.Focus();
    private void ToPetliBez(object sender, EventArgs e) => EntPetliBez.Focus();
    private void ToPetliS(object sender, EventArgs e) => EntPetliS.Focus();
    private void ToSkoba(object sender, EventArgs e) => EntSkoba.Focus();
    private void ToNakladnaya(object sender, EventArgs e) => EntNakladnaya.Focus();
    private void ToKnopka(object sender, EventArgs e) => EntKnopka.Focus();
    private void ToGola(object sender, EventArgs e) => EntGola.Focus();
    private void ToTruba(object sender, EventArgs e) => EntTruba.Focus();
    private void ToGazLift(object sender, EventArgs e) => EntGazLift.Focus();
    private void ToKruchki(object sender, EventArgs e) => EntKruchki.Focus();
    private void ToPodsvetka(object sender, EventArgs e) => EntPodsvetka.Focus();
}