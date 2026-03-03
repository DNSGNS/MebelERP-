using System.Collections.ObjectModel;

namespace MyApp1;

public partial class FasadWarehouseContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }

    public ObservableCollection<FasadForm> StandardFasads => CurrentOrder?.StandardFasads;
    public ObservableCollection<FasadForm> SpecialFasads => CurrentOrder?.SpecialFasads;
    public ObservableCollection<FasadForm> NonStandardFasads => CurrentOrder?.NonStandardFasads;

    // Список типов края для группового выбора
    public List<FasadEdgeType> BulkEdgeTypes { get; } = new() { FasadEdgeType.Freza, FasadEdgeType.Mylo };

    public FasadWarehouseContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;
            if (StandardFasads?.Count == 0) StandardFasads.Add(new FasadForm { SelectedType = FasadType.Standard });
            if (SpecialFasads?.Count == 0) SpecialFasads.Add(new FasadForm { SelectedType = FasadType.AGT });
            if (NonStandardFasads?.Count == 0) NonStandardFasads.Add(new FasadForm { SelectedType = FasadType.NonStandard });
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